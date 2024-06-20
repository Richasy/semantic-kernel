// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Translators.Azure.Core;

internal abstract class ClientBase
{
    private const string TextTranslateEndpoint = "https://api.cognitive.microsofttranslator.com/translate";
    private readonly ILogger _logger;
    private readonly string _version = "3.0";

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

    protected HttpRequestMessage CreateHttpRequest(object requestData, AzureTranslateExecutionSettings requestSettings, string accessKey, string? region = default)
    {
        var queryList = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(requestSettings.From))
        {
            queryList.Add("from", requestSettings.From!);
        }

        queryList.Add("to", requestSettings.To!);
        queryList.Add("api-version", this._version);
        queryList.Add("textType", requestSettings.TextType.ToString().ToLower());

        var uri = new UriBuilder(TextTranslateEndpoint)
        {
            Query = string.Join("&", queryList.Select(p => $"{p.Key}={p.Value}")),
        }.Uri;

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", accessKey);
        if (!string.IsNullOrEmpty(region))
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Region", region);
        }

        var json = JsonSerializer.Serialize(requestData);
        httpRequestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
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
