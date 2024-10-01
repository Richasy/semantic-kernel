// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkImageGenerationClient : ClientBase
{
    private readonly string? _appId;
    private readonly string? _apiKey;
    private readonly string? _secret;
    private readonly string? _version;

    public SparkImageGenerationClient(
        string apiKey,
        string secret,
        string appId,
        string version,
        ILogger? logger = null)
        : base(logger: logger)
    {
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(secret);
        Verify.NotNullOrWhiteSpace(appId);

        this._appId = appId;
        this._apiKey = apiKey;
        this._secret = secret;
        this._version = version;
    }

    public async Task<string> GenerateImageAsync(string prompt, DrawExecutionSettings settings, CancellationToken cancellationToken = default)
    {
        var drawSettings = SparkDeskDrawExecutionSettings.FromExecutionSettings(settings);
        var request = this.GetRequest(prompt, drawSettings.Width, drawSettings.Height);
        var url = GetAuthorizationUrl(this._apiKey!, this._secret!, $"https://spark-api.cn-huabei-1.xf-yun.com/{this._version}/tti", "POST");
        using var httpReq = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonGenContext.Default.SparkImageRequest), System.Text.Encoding.UTF8, "application/json"),
        };
        using var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(httpReq, cancellationToken).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var sparkResponse = JsonSerializer.Deserialize<SparkTextResponse>(responseContent, JsonGenContext.Default.SparkTextResponse);
        if (sparkResponse?.Header?.Code != 0)
        {
            throw new KernelException($"Failed to generate image: {sparkResponse?.Header?.Message}");
        }

        var base64Image = sparkResponse.Payload?.Choices?.Text?.FirstOrDefault()?.Content ?? string.Empty;
        if (string.IsNullOrEmpty(base64Image))
        {
            throw new KernelException("Failed to generate image: empty response");
        }

        return base64Image;
    }

    private SparkImageRequest GetRequest(string description, int width, int height)
    {
        var request = new SparkImageRequest
        {
            Header = new SparkRequestHeader
            {
                AppId = this._appId,
            },

            Parameter = new SparkImageRequest.SparkImageRequestParametersContainer
            {
                Image = new SparkImageRequest.SparkImageRequestParameters
                {
                    Domain = "general",
                    Width = width,
                    Height = height,
                },
            },

            Payload = new SparkTextRequest.SparkTextRequestPayload
            {
                Message = new SparkMessage
                {
                    Text = new List<SparkMessage.SparkTextMessage>
                    {
                        new()
                        {
                            Role = AuthorRole.User,
                            Content = description,
                        }
                    },
                },
            },
        };
        return request;
    }
}
