// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.QianFan.Core;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents a text to image service using QianFan AI.
/// </summary>
public sealed class QianFanImageGenerationService : ITextToImageService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly QianFanImageGenerationClient _imageGenerationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="QianFanImageGenerationService"/> class.
    /// </summary>
    /// <param name="modelId">The QianFan model for the chat completion service.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="apiSecret">Api secret.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the QianFan API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public QianFanImageGenerationService(
        string modelId,
        string apiKey,
        string apiSecret,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(apiSecret);

        this._imageGenerationClient = new QianFanImageGenerationClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            apiKey: apiKey,
            apiSecret: apiSecret,
            logger: loggerFactory?.CreateLogger(typeof(QianFanImageGenerationService)));
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
