// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.QianFan.Core;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents a chat completion service using QianFan API.
/// </summary>
public sealed class QianFanChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly QianFanChatCompletionClient _chatCompletionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="QianFanChatCompletionService"/> class.
    /// </summary>
    /// <param name="modelId">The QianFan model for the chat completion service.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="apiSecret">Api secret.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the QianFan API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public QianFanChatCompletionService(
        string modelId,
        string apiKey,
        string apiSecret,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(apiSecret);

        this._chatCompletionClient = new QianFanChatCompletionClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            apiKey: apiKey,
            apiSecret: apiSecret,
            logger: loggerFactory?.CreateLogger(typeof(QianFanChatCompletionService)));
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
