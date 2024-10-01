// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal sealed class QianFanImageGenerationClient : ClientBase
{
    private readonly string _modelId;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly Uri _imageGenerationEndpoint;

    /// <summary>
    /// Represents a client for interacting with the chat completion QianFan model via BaiduAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="apiKey">Api key for BaiduAI endpoint</param>
    /// <param name="apiSecret">Api secret for BaiduAI</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public QianFanImageGenerationClient(
        HttpClient httpClient,
        string modelId,
        string apiKey,
        string apiSecret,
        ILogger? logger = null)
        : base(
            apiKey: apiKey,
            apiSecret: apiSecret,
            httpClient: httpClient,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);

        this._modelId = modelId;
        this._apiKey = apiKey;
        this._apiSecret = apiSecret;
        this._imageGenerationEndpoint = new Uri($"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/text2image/{this._modelId}");
    }

    /// <summary>
    /// Generates a chat message asynchronously.
    /// </summary>
    /// <param name="prompt">Image prompt.</param>
    /// <param name="executionSettings">Optional settings for prompt execution.</param>
    /// <param name="kernel">A kernel instance.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Returns a list of chat message contents.</returns>
    public async Task<string> GenerateImageAsync(
        string prompt,
        DrawExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        await this.EnsureAuthTokenAsync().ConfigureAwait(false);
        var request = this.GetRequest(prompt, executionSettings);
        using var httpReq = this.CreateHttpRequest(request, JsonGenContext.Default.QianFanImageRequest, this._imageGenerationEndpoint);
        var body = await this.SendRequestAndGetStringBodyAsync(httpReq, cancellationToken).ConfigureAwait(false);
        var response = JsonSerializer.Deserialize<QianFanImageResponse>(body, JsonGenContext.Default.QianFanImageResponse);
        if (response == null || response.Data == null || response.Data.Count == 0)
        {
            throw new KernelException("Failed to generate image: empty response");
        }

        var responseData = response.Data[0].Base64;
        if (string.IsNullOrEmpty(responseData))
        {
            throw new KernelException("Failed to generate image: empty response");
        }

        return responseData!;
    }

    private QianFanImageRequest GetRequest(string prompt, DrawExecutionSettings? settings)
    {
        var drawSettings = QianFanDrawExecutionSettings.FromExecutionSettings(settings);
        return new QianFanImageRequest
        {
            Prompt = prompt,
            Size = $"{drawSettings.Width}x{drawSettings.Height}",
            Number = drawSettings.Number,
            NegativePrompt = drawSettings.NegativePrompt,
        };
    }
}
