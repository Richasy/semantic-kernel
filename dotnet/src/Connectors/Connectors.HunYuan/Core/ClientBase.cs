// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal abstract class ClientBase
{
    private readonly ILogger _logger;
    private readonly string _version = "2023-09-01";
    private readonly string _action = "ChatCompletions";

    protected HttpClient HttpClient { get; }

    protected ClientBase(
        HttpClient httpClient,
        ILogger? logger)
    {
        Verify.NotNull(httpClient);

        this.HttpClient = httpClient;
        this._logger = logger ?? NullLogger.Instance;
    }

    protected static void ValidateMaxTokens(int? maxTokens)
    {
        // If maxTokens is null, it means that the user wants to use the default model value
        if (maxTokens is < 1)
        {
            throw new ArgumentException($"MaxTokens {maxTokens} is not valid, the value must be greater than zero");
        }
    }

    protected async Task<string> SendRequestAndGetStringBodyAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        using var response = await this.HttpClient.SendWithSuccessCheckAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringWithExceptionMappingAsync()
            .ConfigureAwait(false);
        return body;
    }

    protected async Task<HttpResponseMessage> SendRequestAndGetResponseImmediatelyAfterHeadersReadAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        var response = await this.HttpClient.SendWithSuccessCheckAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        return response;
    }

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

    protected HttpRequestMessage CreateHttpRequest(object requestData, Uri endpoint, string secretKey, string secretId)
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        httpRequestMessage.Headers.Add(HttpHeaderConstant.Names.SemanticKernelVersion,
            HttpHeaderConstant.Values.GetAssemblyVersion(typeof(ClientBase)));

        var headers = this.BuildHeaders(endpoint, (HunYuanRequest)requestData, secretKey, secretId);
        foreach (var kvp in headers)
        {
            if (kvp.Key.Equals("Content-Type", StringComparison.Ordinal))
            {
                ByteArrayContent content = new(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(requestData)));
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", kvp.Value);
                httpRequestMessage.Content = content;
            }
            else if (kvp.Key.Equals("Host", StringComparison.Ordinal))
            {
                httpRequestMessage.Headers.Host = kvp.Value;
            }
            else if (kvp.Key.Equals("Authorization", StringComparison.Ordinal))
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("TC3-HMAC-SHA256",
                    kvp.Value.Substring("TC3-HMAC-SHA256".Length));
            }
            else
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
        }

        return httpRequestMessage;
    }

    protected void Log(LogLevel logLevel, string? message, params object[] args)
    {
        if (this._logger.IsEnabled(logLevel))
        {
#pragma warning disable CA2254 // Template should be a constant string.
            this._logger.Log(logLevel, message, args);
#pragma warning restore CA2254
        }
    }

    private Dictionary<string, string> BuildHeaders(Uri endpoint, HunYuanRequest request, string secretKey, string secretId)
    {
        var payload = JsonSerializer.Serialize(request);
        string httpRequestMethod = "POST";
        string canonicalURI = "/";
        string canonicalHeaders = "content-type:" + "application/json" + "\nhost:" + endpoint.Host + $"\nx-tc-action:{this._action.ToLower()}\n";
        string signedHeaders = "content-type;host;x-tc-action";
        string hashedRequestPayload = SignHelper.SHA256Hex(payload);
        string canonicalRequest = httpRequestMethod + "\n"
                                                    + canonicalURI + "\n"
                                                    + string.Empty + "\n"
                                                    + canonicalHeaders + "\n"
                                                    + signedHeaders + "\n"
                                                    + hashedRequestPayload;

        string algorithm = "TC3-HMAC-SHA256";
        var now = DateTimeOffset.Now;
        long timestamp = now.ToUnixTimeSeconds();
        string requestTimestamp = timestamp.ToString();
        string date = now.ToString("yyyy-MM-dd");
        string service = endpoint.Host.Split('.')[0];
        string credentialScope = date + "/" + service + "/" + "tc3_request";
        string hashedCanonicalRequest = SignHelper.SHA256Hex(canonicalRequest);
        string stringToSign = algorithm + "\n"
                                        + requestTimestamp + "\n"
                                        + credentialScope + "\n"
                                        + hashedCanonicalRequest;

        byte[] tc3SecretKey = Encoding.UTF8.GetBytes("TC3" + secretKey);
        byte[] secretDate = SignHelper.HmacSHA256(tc3SecretKey, Encoding.UTF8.GetBytes(date));
        byte[] secretService = SignHelper.HmacSHA256(secretDate, Encoding.UTF8.GetBytes(service));
        byte[] secretSigning = SignHelper.HmacSHA256(secretService, Encoding.UTF8.GetBytes("tc3_request"));
        byte[] signatureBytes = SignHelper.HmacSHA256(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
        string signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

        string authorization = algorithm + " "
                                         + "Credential=" + secretId + "/" + credentialScope + ", "
                                         + "SignedHeaders=" + signedHeaders + ", "
                                         + "Signature=" + signature;

        Dictionary<string, string> headers = new()
        {
            { "Authorization", authorization },
            { "Host", endpoint.Host },
            { "Content-Type", "application/json" },
            { "X-TC-Timestamp", requestTimestamp },
            { "X-TC-Version", this._version },
            { "X-TC-Action", this._action },
        };

        return headers;
    }
}
