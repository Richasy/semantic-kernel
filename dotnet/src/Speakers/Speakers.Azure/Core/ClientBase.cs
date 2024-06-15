// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.SemanticKernel.Http;
using System.Security;

namespace Microsoft.SemanticKernel.Speakers.Azure.Core;

internal abstract class ClientBase
{
    private readonly string _speechEndpoint;
    private readonly ILogger _logger;

    protected ClientBase(
        string region,
        HttpClient httpClient,
        ILogger? logger)
    {
        this._speechEndpoint = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";
        this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this._logger = logger ?? NullLogger.Instance;
    }

    protected HttpClient HttpClient { get; }

    protected static T DeserializeResponse<T>(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(body) ?? throw new JsonException("Response is null");
        }
        catch (JsonException exc)
        {
            throw new KernelException("Unexpected response from model", exc)
            {
                Data = { { "ResponseData", body } },
            };
        }
    }

    protected async Task<byte[]> SendRequestAndGetBytesAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        using var response = await this.HttpClient.SendWithSuccessCheckAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsByteArrayAsync()
            .ConfigureAwait(false);
        return body;
    }

    protected HttpRequestMessage CreateHttpRequest(string text, AzureTextToAudioExecutionSettings requestSettings, string accessKey)
    {
        var uri = new UriBuilder(this._speechEndpoint).Uri;
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", accessKey);
        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        httpRequestMessage.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");

        var content = $"""
            <speak version='1.0' xml:lang='{requestSettings.Language}'>
            <voice xml:lang='{requestSettings.Language}' xml:gender='{requestSettings.Gender}' name='{requestSettings.Voice}'>
            <prosody rate='{requestSettings.Speed}'></prosody>
            {SecurityElement.Escape(text)}
            </voice>
            </speak>
            """;
        httpRequestMessage.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/ssml+xml");
        return httpRequestMessage;
    }

    protected void Log(LogLevel logLevel, string? message, params object[] args)
    {
        if (this._logger.IsEnabled(logLevel))
        {
            this._logger.Log(logLevel, message, args);
        }
    }
}
