// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.DouBao.Core;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Connectors.DouBao;

/// <summary>
/// Represents a chat completion service using DouBao API.
/// </summary>
public sealed class DouBaoChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly DouBaoChatCompletionClient _chatCompletionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DouBaoChatCompletionService"/> class.
    /// </summary>
    /// <param name="modelId">The DouBao model for the chat completion service.</param>
    /// <param name="token">The API key for authentication.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the DouBao API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public DouBaoChatCompletionService(
        string modelId,
        string token,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(token);

        this._chatCompletionClient = new DouBaoChatCompletionClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            token: token,
            logger: loggerFactory?.CreateLogger(typeof(DouBaoChatCompletionService)));
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
