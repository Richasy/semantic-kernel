// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Represents a chat completion service using Spark Desk AI.
/// </summary>
public sealed class SparkDeskChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly SparkChatCompletionClient _chatCompletionClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkDeskChatCompletionService"/> class.
    /// </summary>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="secret">Secret.</param>
    /// <param name="appId">App id.</param>
    /// <param name="apiVersion">Version of the Google API</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public SparkDeskChatCompletionService(
        string apiKey,
        string secret,
        string appId,
        SparkDeskAIVersion apiVersion = SparkDeskAIVersion.V3_5,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(apiKey);

        this._chatCompletionClient = new SparkChatCompletionClient(
            apiKey: apiKey,
            secret: secret,
            appId: appId,
            apiVersion: apiVersion,
            logger: loggerFactory?.CreateLogger(typeof(SparkDeskChatCompletionService)));
        this._attributesInternal.Add(AIServiceExtensions.ModelIdKey, apiVersion.ToString());
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
