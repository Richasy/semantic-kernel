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

namespace Microsoft.SemanticKernel.Translators.Volcano.Core;

internal abstract class ClientBase
{
    private const string TextTranslateEndpoint = "https://translate.volcengineapi.com";
    private const string Action = "TranslateText";
    private const string Version = "2020-06-01";
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

    protected HttpRequestMessage CreateHttpRequest(object requestData, string secretKey, string secretId, string region = "cn-north-1")
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var endpoint = new Uri($"{TextTranslateEndpoint}?Action={Action}&Version={Version}");
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        httpRequestMessage.Headers.Add(HttpHeaderConstant.Names.SemanticKernelVersion,
            HttpHeaderConstant.Values.GetAssemblyVersion(typeof(ClientBase)));

        var headers = this.BuildHeaders(endpoint, (TextTranslateRequest)requestData, secretKey, secretId, region);
        foreach (var kvp in headers)
        {
            if (kvp.Key.Equals("Authorization", StringComparison.Ordinal))
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("HMAC-SHA256",
                    kvp.Value.Substring("HMAC-SHA256".Length + 1));
            }
            else if (kvp.Key.Equals("Host", StringComparison.Ordinal))
            {
                httpRequestMessage.Headers.Host = kvp.Value;
            }
            else
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
        }

        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
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
        var query = endpoint.Query.TrimStart('?');
        var now = DateTime.UtcNow;
        var dateFull = now.ToString("yyyyMMddTHHmmssZ");
        var date = now.ToString("yyyyMMdd");
        string httpRequestMethod = "POST";
        string canonicalURI = "/";
        string canonicalHeaders = "host:" + endpoint.Host + $"\nx-date:{dateFull}\n";
        string signedHeaders = "host;x-date";
        string hashedRequestPayload = SignHelper.SHA256Hex(payload);
        string canonicalRequest = httpRequestMethod + "\n"
                                                    + canonicalURI + "\n"
                                                    + query + "\n"
                                                    + canonicalHeaders + "\n"
                                                    + signedHeaders + "\n"
                                                    + hashedRequestPayload;
        string algorithm = "HMAC-SHA256";
        string service = endpoint.Host.Split('.')[0];
        string credentialScope = date + "/" + region + "/" + service + "/" + "request";
        string hashedCanonicalRequest = SignHelper.SHA256Hex(canonicalRequest);
        string stringToSign = algorithm + "\n"
                                        + dateFull + "\n"
                                        + credentialScope + "\n"
                                        + hashedCanonicalRequest;

        byte[] secretKeyHash = Encoding.UTF8.GetBytes(secretKey);
        byte[] secretDate = SignHelper.HmacSHA256(secretKeyHash, Encoding.UTF8.GetBytes(date));
        byte[] secretRegion = SignHelper.HmacSHA256(secretDate, Encoding.UTF8.GetBytes(region));
        byte[] secretService = SignHelper.HmacSHA256(secretRegion, Encoding.UTF8.GetBytes(service));
        byte[] secretSigning = SignHelper.HmacSHA256(secretService, Encoding.UTF8.GetBytes("request"));
        byte[] signatureBytes = SignHelper.HmacSHA256(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
        string signature = SignHelper.HexEncode(signatureBytes);

        string authorization = algorithm + " "
                                         + "Credential=" + secretId + "/" + credentialScope + ", "
                                         + "SignedHeaders=" + signedHeaders + ", "
                                         + "Signature=" + signature;

        Dictionary<string, string> headers = new()
        {
            { "Authorization", authorization },
            { "Host", endpoint.Host },
            { "X-Date", dateFull }
        };

        return headers;
    }
}
