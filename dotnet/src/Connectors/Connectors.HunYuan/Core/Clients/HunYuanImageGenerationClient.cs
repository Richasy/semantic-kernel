// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

/// <summary>
/// Client for generating images using the HunYuan model.
/// </summary>
internal sealed class HunYuanImageGenerationClient : ClientBase
{
    private readonly string _modelId;
    private readonly string _secretId;
    private readonly string _secretKey;
    private readonly Uri _imageGenerationEndpoint;

    /// <summary>
    /// Represents a client for interacting with the chat completion HunYuan model via BaiduAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="secretId">App Id in Tencent Cloud.</param>
    /// <param name="secretKey">Api key for Tencent Cloud endpoint</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public HunYuanImageGenerationClient(
        HttpClient httpClient,
        string modelId,
        string secretId,
        string secretKey,
        ILogger? logger = null)
        : base(
            httpClient: httpClient,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(secretKey);
        Verify.NotNullOrWhiteSpace(secretId);

        this._modelId = modelId;
        this._secretKey = secretKey;
        this._secretId = secretId;
        this._imageGenerationEndpoint = new Uri("https://hunyuan.tencentcloudapi.com");
    }

    public async Task<string> GenerateImageAsync(
        string prompt,
        DrawExecutionSettings? settings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var request = this.CreateDrawRequest(prompt, settings);
        using var httpReq = this.CreateHttpRequest(request, this._imageGenerationEndpoint, this._secretKey, this._secretId, "SubmitHunyuanImageJob", region: "ap-guangzhou");
        var body = await this.SendRequestAndGetStringBodyAsync(httpReq, cancellationToken).ConfigureAwait(false);
        var createResponse = JsonSerializer.Deserialize<HunYuanDrawCreateResponse>(body);
        if (createResponse is null || string.IsNullOrEmpty(createResponse?.Response?.JobId))
        {
            throw new KernelException("Failed to create hunyuan draw job.");
        }

        var jobId = createResponse!.Response!.JobId;
        do
        {
            var (image, finished) = await this.GetJobResultAsync(jobId!, cancellationToken).ConfigureAwait(false);
            if (finished)
            {
                if (string.IsNullOrEmpty(image))
                {
                    throw new KernelException("Failed to get image from HunYuan draw job.");
                }

                return image;
            }

            await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
        } while (!cancellationToken.IsCancellationRequested);

        throw new KernelException("HunYuan draw job cancelled.");
    }

    /// <summary>
    /// Get the result of a HunYuan image generation job.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>First parameter is image link, second parameter indicate the job was finished.</returns>
    private async Task<(string, bool)> GetJobResultAsync(string jobId, CancellationToken cancellationToken)
    {
        var request = new HunYuanDrawQueryRequest
        {
            JobId = jobId,
        };
        using var httpReq = this.CreateHttpRequest(request, this._imageGenerationEndpoint, this._secretKey, this._secretId, "QueryHunyuanImageJob", region: "ap-guangzhou");
        var body = await this.SendRequestAndGetStringBodyAsync(httpReq, cancellationToken).ConfigureAwait(false);
        var response = JsonSerializer.Deserialize<HunYuanDrawQueryResponse>(body);

        // Task failed.
        if (response is null || response.Response!.JobStatusCode == "4")
        {
            return (string.Empty, true);
        }

        // Success.
        if (response.Response!.JobStatusCode == "5")
        {
            var image = response.Response!.ResultImage.FirstOrDefault();
            return (image, true);
        }

        return (string.Empty, false);
    }

    private HunYuanDrawCreateRequest CreateDrawRequest(string prompt, DrawExecutionSettings? settings)
    {
        var drawSettings = HunYuanDrawExecutionSettings.FromExecutionSettings(settings);
        return new HunYuanDrawCreateRequest
        {
            Prompt = prompt,
            Resolution = $"{drawSettings.Width}:{drawSettings.Height}",
        };
    }
}
