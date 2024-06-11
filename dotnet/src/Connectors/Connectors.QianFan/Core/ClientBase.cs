// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal abstract class ClientBase
{
    private readonly ILogger _logger;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private QianFanTokenContext? _token;

    protected HttpClient HttpClient { get; }

    protected ClientBase(
        string apiKey,
        string apiSecret,
        HttpClient httpClient,
        ILogger? logger)
    {
        Verify.NotNull(httpClient);
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(apiSecret);

        this.HttpClient = httpClient;
        this._apiKey = apiKey;
        this._apiSecret = apiSecret;
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

    protected HttpRequestMessage CreateHttpRequest(object requestData, Uri endpoint)
    {
        var token = this._token?.Token?.AccessToken ?? throw new InvalidOperationException("Token is not initialized.");
        endpoint = new Uri(endpoint.AbsoluteUri.TrimEnd('/') + $"?access_token={token}");
        var httpRequestMessage = HttpRequest.CreatePostRequest(endpoint, requestData);
        httpRequestMessage.Headers.Add("User-Agent", HttpHeaderConstant.Values.UserAgent);
        httpRequestMessage.Headers.Add(HttpHeaderConstant.Names.SemanticKernelVersion,
            HttpHeaderConstant.Values.GetAssemblyVersion(typeof(ClientBase)));

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

    protected async Task EnsureAuthTokenAsync()
    {
        if (this._token == null || !this._token.IsValid)
        {
            this._token = new QianFanTokenContext(await CreateAuthTokenAsync(this._apiKey, this._apiSecret).ConfigureAwait(false), DateTime.Now);
        }
    }

    private static async Task<QianFanAuthToken> CreateAuthTokenAsync(string apiKey, string apiSecret, CancellationToken cancellationToken = default)
    {
        using HttpClient http = new();
        string apiUri = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={apiKey}&client_secret={apiSecret}";
        HttpResponseMessage resp = await http.GetAsync(apiUri, cancellationToken).ConfigureAwait(false);

        if (resp.IsSuccessStatusCode)
        {
            var stringContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var token = JsonSerializer.Deserialize<QianFanAuthToken>(stringContent);
            return token ?? throw new KernelException($"Unable to deserialize '{await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}' into {nameof(QianFanAuthToken)}.");
        }

        throw new HttpRequestException($"{resp.ReasonPhrase}: {await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
    }
}
