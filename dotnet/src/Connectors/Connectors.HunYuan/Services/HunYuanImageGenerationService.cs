// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HunYuan.Core;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Represents a chat completion service using HunYuan API.
/// </summary>
public sealed class HunYuanImageGenerationService : ITextToImageService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly HunYuanImageGenerationClient _imageGenerationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HunYuanImageGenerationService"/> class.
    /// </summary>
    /// <param name="modelId">The HunYuan model for the chat completion service.</param>
    /// <param name="secretId">The API key for authentication.</param>
    /// <param name="secretKey">Api secret.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the HunYuan API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public HunYuanImageGenerationService(
        string modelId,
        string secretId,
        string secretKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(secretId);
        Verify.NotNullOrWhiteSpace(secretKey);

        this._imageGenerationClient = new HunYuanImageGenerationClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            secretId: secretId,
            secretKey: secretKey,
            logger: loggerFactory?.CreateLogger(typeof(HunYuanImageGenerationService)));
        this._attributesInternal.Add(AIServiceExtensions.ModelIdKey, modelId);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc/>
    public Task<string> GenerateImageAsync(string description, DrawExecutionSettings settings, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        return this._imageGenerationClient.GenerateImageAsync(description, settings, kernel, cancellationToken);
    }
}
