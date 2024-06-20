// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Translators.Tencent.Core;

internal abstract class ClientBase
{
    private const string TextTranslateEndpoint = "https://tmt.tencentcloudapi.com";
    private const string Action = "TextTranslate";
    private const string Version = "2018-03-21";
    private readonly ILogger _logger;

    protected ClientBase(
        HttpClient httpClient,
        ILogger? logger)
    {
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

    protected HttpRequestMessage CreateHttpRequest(object requestData, string secretKey, string secretId, string region = "ap-beijing")
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var endpoint = new Uri(TextTranslateEndpoint);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        httpRequestMessage.Headers.Add(HttpHeaderConstant.Names.SemanticKernelVersion,
            HttpHeaderConstant.Values.GetAssemblyVersion(typeof(ClientBase)));

        var headers = this.BuildHeaders(endpoint, (TextTranslateRequest)requestData, secretKey, secretId, region);
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
                    kvp.Value.Substring("TC3-HMAC-SHA256".Length).Trim());
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
            this._logger.Log(logLevel, message, args);
        }
    }

    private Dictionary<string, string> BuildHeaders(Uri endpoint, TextTranslateRequest request, string secretKey, string secretId, string region)
    {
        var payload = JsonSerializer.Serialize(request);
        string httpRequestMethod = "POST";
        string canonicalURI = "/";
        string canonicalHeaders = "content-type:" + "application/json" + "\nhost:" + endpoint.Host + $"\nx-tc-action:{Action.ToLower()}\n";
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
            { "X-TC-Version", Version },
            { "X-TC-Action", Action },
            { "X-TC-Region", region }
        };

        return headers;
    }
}
