// Copyright (c) Richasy. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Translators.Baidu.Core;

internal abstract class ClientBase
{
    private const string TextTranslateEndpoint = "https://fanyi-api.baidu.com/api/trans/vip/translate";
    private readonly ILogger _logger;
    private readonly string _appId;
    private readonly string _secret;
    private readonly string _salt;

    protected ClientBase(
        HttpClient httpClient,
        ILogger? logger,
        string appId,
        string secret)
    {
        this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this._logger = logger ?? NullLogger.Instance;
        this._appId = appId;
        this._secret = secret;
        this._salt = new Random().Next(100000).ToString();
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

    protected HttpRequestMessage CreateHttpRequest(string input, BaiduTranslateExecutionSettings requestSettings)
    {
        var queryList = new Dictionary<string, string>();
        var from = string.IsNullOrEmpty(requestSettings.From) ? "auto" : requestSettings.From;
        queryList.Add("q", input);
        queryList.Add("from", from!);
        queryList.Add("to", requestSettings.To!);
        queryList.Add("appid", this._appId);
        queryList.Add("salt", this._salt);
        queryList.Add("sign", this.GenerateSign(input));

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, TextTranslateEndpoint);
        httpRequestMessage.Content = new FormUrlEncodedContent(queryList);
        return httpRequestMessage;
    }

    protected void Log(LogLevel logLevel, string? message, params object[] args)
    {
        if (this._logger.IsEnabled(logLevel))
        {
            this._logger.Log(logLevel, message, args);
        }
    }

    private string GenerateSign(string input)
    {
        var byteOld = Encoding.UTF8.GetBytes(this._appId + input + this._salt + this._secret);
        using var md5 = System.Security.Cryptography.MD5.Create();
        var byteNew = md5.ComputeHash(byteOld);
        var sign = new StringBuilder();
        foreach (var t in byteNew)
        {
            sign.Append(t.ToString("x2"));
        }

        return sign.ToString();
    }
}
