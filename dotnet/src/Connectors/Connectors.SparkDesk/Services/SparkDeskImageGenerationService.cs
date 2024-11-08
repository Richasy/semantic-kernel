// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.SparkDesk.Core;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

internal class SparkDeskImageGenerationService : ITextToImageService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly SparkImageGenerationClient _imageGenerationClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkDeskImageGenerationService"/> class.
    /// </summary>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="secret">Secret.</param>
    /// <param name="appId">App id.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public SparkDeskImageGenerationService(
        string apiKey,
        string secret,
        string appId,
        string version,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(apiKey);

        this._imageGenerationClient = new SparkImageGenerationClient(
            apiKey: apiKey,
            secret: secret,
            appId: appId,
            version: version,
            logger: loggerFactory?.CreateLogger(typeof(SparkDeskChatCompletionService)));
        this._attributesInternal.Add(AIServiceExtensions.ModelIdKey, version);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    public Task<string> GenerateImageAsync(string description, DrawExecutionSettings settings, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        return this._imageGenerationClient.GenerateImageAsync(description, settings, cancellationToken);
    }

    public Task<string> GenerateImageAsync(string description, int width, int height, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        return this._imageGenerationClient.GenerateImageAsync(description, new SparkDeskDrawExecutionSettings { Width = width, Height = height }, cancellationToken);
    }

    public Task<IReadOnlyList<ImageContent>> GetImageContentsAsync(TextContent input, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
