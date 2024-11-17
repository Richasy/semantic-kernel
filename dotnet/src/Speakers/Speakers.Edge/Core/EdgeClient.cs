// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Speakers.Edge.Core;

internal sealed class EdgeClient
{
    public async Task<AudioContent> GenerateAudioAsync(string text, EdgeTextToAudioExecutionSettings settings, CancellationToken? cancellationToken = default)
    {
        const string BinaryDelim = "Path:audio\r\n";
        var sendRequestId = Guid.NewGuid().ToString("N");
        var binary = new List<byte>();

        var taskCompletionSource = new TaskCompletionSource<AudioContent>();
        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri($"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken=6A5AA1D4EAFF4E9FB37E23D68491D6F4&Sec-MS-GEC={GenerateSecMsGecToken()}&Sec-MS-GEC-Version=1-130.0.2849.68"), cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        var receiveTask = Task.Run(async () =>
        {
            var buffer = new byte[1024 * 4];
            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var data = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (data.Contains("Path:turn.end"))
                    {
                        if (binary.Count > 0)
                        {
                            var content = new AudioContent(new ReadOnlyMemory<byte>(binary.ToArray()), "audio/mp3");
                            taskCompletionSource.SetResult(content);
                        }
                        else
                        {
                            taskCompletionSource.SetException(new KernelException("Edge speech result is empty."));
                        }
                        break;
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var data = new ArraySegment<byte>(buffer, 0, result.Count).ToArray();
                    if (data.Length >= 3 && data[0] == 0x00 && data[1] == 0x67 && data[2] == 0x58)
                    {
                        // Last (empty) audio fragment.
                    }
                    else
                    {
                        var index = Encoding.UTF8.GetString(data).IndexOf(BinaryDelim) + BinaryDelim.Length;
                        if (index < BinaryDelim.Length)
                        {
                            binary.AddRange(data);
                        }
                        else
                        {
                            var curVal = data[index..];
                            binary.AddRange(curVal);
                        }
                    }
                }
            }
        });

        await Task.Run(() =>
        {
            client.SendAsync(Encoding.UTF8.GetBytes(ConvertToAudioFormatWebSocketString(settings.Codec)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            if (text.StartsWith("<speak"))
            {
                client.SendAsync(Encoding.UTF8.GetBytes(ConvertToWebSocketString(sendRequestId, text)), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }
            else
            {
                client.SendAsync(Encoding.UTF8.GetBytes(ConvertToWebSocketString(sendRequestId, ConvertToSsmlText(settings.Language, settings.Voice, settings.Speed, text))), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }
        }).ConfigureAwait(false);

        await receiveTask.ConfigureAwait(false);
        return await taskCompletionSource.Task.ConfigureAwait(false);
    }

    private static string GenerateSecMsGecToken()
    {
        var ticks = DateTime.Now.ToFileTimeUtc();
        ticks -= ticks % 3_000_000_000;
        return ToHexString(HashData(Encoding.ASCII.GetBytes(ticks + "6A5AA1D4EAFF4E9FB37E23D68491D6F4")));
    }

    private static string ToHexString(byte[] byteArray)
    {
        return Convert.ToHexString(byteArray).ToUpperInvariant();
    }

    private static byte[] HashData(byte[] data)
    {
        return SHA256.HashData(data);
    }

    private static string ConvertToSsmlText(string lang, string voice, double speed, string text)
    {
        return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'  xml:lang='{lang}'><voice name='{voice}'><prosody pitch='+0Hz' rate ='{FromatPercentage(speed)}'>{text}</prosody></voice></speak>";
    }

    private static string ConvertToAudioFormatWebSocketString(string outputformat)
    {
        return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputformat + "\"}}}}";
    }

    private static string ConvertToWebSocketString(string requestId, string msg)
    {
        return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{msg}";
    }

    private static string FromatPercentage(double input)
    {
        return input < 0 ? input.ToString("+#;-#;0") + "%" : input.ToString("+#;-#;0") + "%";
    }
}
