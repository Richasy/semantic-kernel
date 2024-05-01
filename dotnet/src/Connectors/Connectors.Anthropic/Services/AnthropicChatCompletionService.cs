// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Services;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Connectors.Anthropic.Core;
using System;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

/// <summary>
/// Represents a chat completion service using Anthropic API.
/// </summary>
public sealed class AnthropicChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly AnthropicChatCompletionClient _chatCompletionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnthropicChatCompletionService"/> class.
    /// </summary>
    /// <param name="modelId">The Anthropic model for the chat completion service.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="baseUrl">Base url.</param>
    /// <param name="httpClient">Optional HTTP client to be used for communication with the Anthropic API.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public AnthropicChatCompletionService(
        string modelId,
        string apiKey,
        Uri? baseUrl = null,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(apiKey);

        this._chatCompletionClient = new AnthropicChatCompletionClient(
#pragma warning disable CA2000
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
#pragma warning restore CA2000
            modelId: modelId,
            apiKey: apiKey,
            baseUrl: baseUrl,
            logger: loggerFactory?.CreateLogger(typeof(AnthropicChatCompletionService)));
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
