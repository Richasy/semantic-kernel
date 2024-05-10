// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.HunYuan.Core;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Represents a chat completion service using HunYuan API.
/// </summary>
public sealed class HunYuanChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly HunYuanChatCompletionClient _chatCompletionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HunYuanChatCompletionService"/> class.
    /// </summary>
    /// <param name="modelId">The HunYuan model for the chat completion service.</param>
    /// <param name="secretId">The API key for authentication.</param>
    /// <param name="secretKey">Api secret.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the HunYuan API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public HunYuanChatCompletionService(
        string modelId,
        string secretId,
        string secretKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(secretId);
        Verify.NotNullOrWhiteSpace(secretKey);

        this._chatCompletionClient = new HunYuanChatCompletionClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            secretId: secretId,
            secretKey: secretKey,
            logger: loggerFactory?.CreateLogger(typeof(HunYuanChatCompletionService)));
        this._attributesInternal.Add(AIServiceExtensions.ModelIdKey, modelId);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc />
    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return this._chatCompletionClient.GenerateChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return this._chatCompletionClient.StreamGenerateChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }
}
