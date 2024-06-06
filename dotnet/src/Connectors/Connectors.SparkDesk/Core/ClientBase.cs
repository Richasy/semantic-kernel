// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal abstract class ClientBase
{
    private const string BaseUrl = "wss://spark-api.xf-yun.com";
    private readonly ILogger _logger;

    protected ClientBase(ILogger? logger)
    {
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

    protected static string GetAuthorizationUrl(string apiKey, string secret, string hostUrl)
    {
        var url = new Uri(hostUrl);
        var dateString = DateTime.UtcNow.ToString("r");
        var signatureBytes = Encoding.ASCII.GetBytes($"host: {url.Host}\ndate: {dateString}\nGET {url.AbsolutePath} HTTP/1.1");

        using HMACSHA256 hmacsha256 = new(Encoding.ASCII.GetBytes(secret));
        var computedHash = hmacsha256.ComputeHash(signatureBytes);
        var signature = Convert.ToBase64String(computedHash);

        var authorizationString = $"api_key=\"{apiKey}\",algorithm=\"hmac-sha256\",headers=\"host date request-line\",signature=\"{signature}\"";
        var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(authorizationString));

        var query = $"authorization={authorization}&date={dateString}&host={url.Host}";

        return new UriBuilder(url) { Scheme = url.Scheme, Query = query }.ToString();
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

    protected static string GetHostApi(SparkDeskAIVersion version)
        => version switch
        {
            SparkDeskAIVersion.V1_5 => $"{BaseUrl}/v1.1",
            SparkDeskAIVersion.V2 => $"{BaseUrl}/v2.1",
            SparkDeskAIVersion.V3 => $"{BaseUrl}/v3.1",
            SparkDeskAIVersion.V3_5 => $"{BaseUrl}/v3.5",
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, "Unknown version"),
        };
}
