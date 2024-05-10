// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using LLama.Abstractions;
using Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;
using static LLama.LLamaTransforms;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp;

/// <summary>
/// Represents a chat completion service using LlamaSharp.
/// </summary>
public sealed class LlamaSharpChatCompletionService : IChatCompletionService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly ILLamaExecutor _interactiveExecutor;
    private readonly IHistoryTransform _historyTransform;
    private readonly ITextStreamTransform _outputTransform;

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
        var context = model.CreateContext(parameters, loggerFactory?.CreateLogger<LlamaSharpChatCompletionService>());
        this._interactiveExecutor = new InteractiveExecutor(context);
        this._historyTransform = this.GetHistoryTransform(modelPath);
        this._outputTransform = new KeywordTextOutputStreamTransform(this.GetKeywords(modelPath));
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
        await foreach (var token in this.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken).ConfigureAwait(false))
        {
            sb.Append(token.Content);
        }

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
        var prompt = this.GetFormattedPrompt(chatHistory);
        var result = this._interactiveExecutor.InferAsync(prompt, settings.ToLLamaSharpInferenceParams(), cancellationToken);
        var output = this._outputTransform.TransformAsync(result);
        await foreach (var token in output.ConfigureAwait(false))
        {
            yield return new StreamingChatMessageContent(ChatCompletion.AuthorRole.Assistant, token);
        }
    }

    private string GetFormattedPrompt(ChatCompletion.ChatHistory chatHistory)
    {
        string prompt;
        prompt = this._historyTransform.HistoryToText(chatHistory.ToLLamaSharpChatHistory());
        return prompt;
    }

    private IHistoryTransform GetHistoryTransform(string modelPath)
    {
        var modelType = this.GetModelTemplateType(modelPath);
        return modelType switch
        {
            ModelTemplateType.Phi => new PhiHistoryTransform(),
            _ => new BasicHistoryTransform(),
        };
    }

    private IEnumerable<string> GetKeywords(string modelPath)
    {
        var modelType = this.GetModelTemplateType(modelPath);
        return modelType switch
        {
            ModelTemplateType.Phi => PhiHistoryTransform.Keywrods,
            _ => [ $"{LLama.Common.AuthorRole.User}:",
                   $"{LLama.Common.AuthorRole.Assistant}:",
                   $"{LLama.Common.AuthorRole.System}:"],
        };
    }

    private ModelTemplateType GetModelTemplateType(string modelPath)
    {
        var modelName = Path.GetFileNameWithoutExtension(modelPath);
        if (modelName.StartsWith("phi", System.StringComparison.InvariantCultureIgnoreCase))
        {
            return ModelTemplateType.Phi;
        }

        return ModelTemplateType.Basic;
    }
}
