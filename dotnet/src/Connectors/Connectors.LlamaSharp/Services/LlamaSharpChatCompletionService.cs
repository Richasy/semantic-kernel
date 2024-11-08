﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;
using Microsoft.SemanticKernel.Services;
using static LLama.LLamaTransforms;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp;

/// <summary>
/// Represents a chat completion service using LlamaSharp.
/// </summary>
public sealed class LlamaSharpChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly LLamaContext _context;
    private readonly InteractiveExecutor _interactiveExecutor;
    private BasicHistoryTransform _historyTransform;
    private KeywordTextOutputStreamTransform _outputTransform;

    /// <summary>
    /// Initializes a new instance of the <see cref="LlamaSharpChatCompletionService"/> class.
    /// </summary>
    /// <param name="modelPath">The local model for the chat completion service.</param>
    /// <param name="loggerFactory">Optional logger factory to be used for logging.</param>
    public LlamaSharpChatCompletionService(
        string modelPath,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(modelPath);

        var parameters = new ModelParams(modelPath)
        {
            GpuLayerCount = 1000,
            ContextSize = 4000,
        };

        using var model = LLamaWeights.LoadFromFile(parameters);
        this._context = model.CreateContext(parameters, loggerFactory?.CreateLogger<LlamaSharpChatCompletionService>());
        this._interactiveExecutor = new InteractiveExecutor(this._context);
        var historyTransform = new BasicHistoryTransform();
        this._historyTransform = historyTransform;
        this._outputTransform = new KeywordTextOutputStreamTransform(historyTransform.Keywords!);
        this._attributesInternal.Add(AIServiceExtensions.ModelIdKey, modelPath);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatCompletion.ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        await Task.Run(async () =>
        {
            await foreach (var token in this.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken).ConfigureAwait(false))
            {
                sb.Append(token.Content);
            }
        }, cancellationToken).ConfigureAwait(false);

        return new List<ChatMessageContent> { new(ChatCompletion.AuthorRole.Assistant, sb.ToString()) }.AsReadOnly();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatCompletion.ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var settings = LlamaSharpPromptExecutionSettings.FromExecutionSettings(executionSettings);
        if (!string.IsNullOrEmpty(settings.SystemTemplate)
            || !string.IsNullOrEmpty(settings.UserTemplate)
            || !string.IsNullOrEmpty(settings.AssistantTemplate)
            || !string.IsNullOrEmpty(settings.EndTemplate))
        {
            var historyTransform = new BasicHistoryTransform(settings.SystemTemplate, settings.UserTemplate, settings.AssistantTemplate, settings.EndTemplate);
            this._outputTransform = new KeywordTextOutputStreamTransform(historyTransform.Keywords!);
            this._historyTransform = historyTransform;
        }

        var prompt = this.GetFormattedPrompt(chatHistory);
        var result = this._interactiveExecutor.InferAsync(prompt, settings.ToLLamaSharpInferenceParams(), cancellationToken);
        var output = this._outputTransform.TransformAsync(result).ConfigureAwait(false);
        await foreach (var token in output.ConfigureAwait(false))
        {
            yield return new StreamingChatMessageContent(ChatCompletion.AuthorRole.Assistant, token);
        }
    }

    /// <summary>
    /// Releases the resources used by the <see cref="LlamaSharpChatCompletionService"/>.
    /// </summary>
    public void Release()
    {
        this._context?.Dispose();
    }

    private string GetFormattedPrompt(ChatCompletion.ChatHistory chatHistory)
    {
        string prompt;
        prompt = this._historyTransform.HistoryToText(chatHistory.ToLLamaSharpChatHistory());
        return prompt;
    }
}