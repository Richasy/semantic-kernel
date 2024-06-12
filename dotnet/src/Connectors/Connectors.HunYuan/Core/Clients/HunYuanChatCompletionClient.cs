// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

/// <summary>
/// Represents a client for interacting with the chat completion HunYuan model.
/// </summary>
internal sealed class HunYuanChatCompletionClient : ClientBase
{
    private readonly StreamJsonParser _streamJsonParser = new();
    private readonly string _modelId;
    private readonly string _secretId;
    private readonly string _secretKey;
    private readonly Uri _chatGenerationEndpoint;
    private static readonly string s_namespace = typeof(HunYuanChatCompletionClient).Namespace!;

    /// <summary>
    /// Instance of <see cref="Meter"/> for metrics.
    /// </summary>
    private static readonly Meter s_meter = new(s_namespace);

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the number of prompt tokens used.
    /// </summary>
    private static readonly Counter<int> s_promptTokensCounter =
        s_meter.CreateCounter<int>(
            name: $"{s_namespace}.tokens.prompt",
            unit: "{token}",
            description: "Number of prompt tokens used");

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the number of completion tokens used.
    /// </summary>
    private static readonly Counter<int> s_completionTokensCounter =
        s_meter.CreateCounter<int>(
            name: $"{s_namespace}.tokens.completion",
            unit: "{token}",
            description: "Number of completion tokens used");

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the total number of tokens used.
    /// </summary>
    private static readonly Counter<int> s_totalTokensCounter =
        s_meter.CreateCounter<int>(
            name: $"{s_namespace}.tokens.total",
            unit: "{token}",
            description: "Number of tokens used");

    /// <summary>
    /// Represents a client for interacting with the chat completion HunYuan model via BaiduAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="secretId">App Id in Tencent Cloud.</param>
    /// <param name="secretKey">Api key for Tencent Cloud endpoint</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public HunYuanChatCompletionClient(
        HttpClient httpClient,
        string modelId,
        string secretId,
        string secretKey,
        ILogger? logger = null)
        : base(
            httpClient: httpClient,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(secretKey);
        Verify.NotNullOrWhiteSpace(secretId);

        this._modelId = modelId;
        this._secretKey = secretKey;
        this._secretId = secretId;
        this._chatGenerationEndpoint = new Uri("https://hunyuan.tencentcloudapi.com");
    }

    /// <summary>
    /// Generates a chat message asynchronously.
    /// </summary>
    /// <param name="chatHistory">The chat history containing the conversation data.</param>
    /// <param name="executionSettings">Optional settings for prompt execution.</param>
    /// <param name="kernel">A kernel instance.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Returns a list of chat message contents.</returns>
    public async Task<IReadOnlyList<ChatMessageContent>> GenerateChatMessageAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var state = this.ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);
        var response = await this.SendRequestAndReturnValidHunYuanResponseAsync(
                    this._chatGenerationEndpoint, state.HunYuanRequest, cancellationToken)
                .ConfigureAwait(false);

        var chatResponses = this.ProcessChatResponse(response.Response!);
        return chatResponses;
    }

    /// <summary>
    /// Generates a stream of chat messages asynchronously.
    /// </summary>
    /// <param name="chatHistory">The chat history containing the conversation data.</param>
    /// <param name="executionSettings">Optional settings for prompt execution.</param>
    /// <param name="kernel">A kernel instance.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of <see cref="StreamingChatMessageContent"/> streaming chat contents.</returns>
    public async IAsyncEnumerable<StreamingChatMessageContent> StreamGenerateChatMessageAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = this.ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);

        for (state.Iteration = 1; ; state.Iteration++)
        {
            state.HunYuanRequest.Stream = true;
            using var httpRequestMessage = this.CreateHttpRequest(state.HunYuanRequest, this._chatGenerationEndpoint, this._secretKey, this._secretId);
            using var response = await this.SendRequestAndGetResponseImmediatelyAfterHeadersReadAsync(httpRequestMessage, cancellationToken)
                .ConfigureAwait(false);
            using var responseStream = await response.Content.ReadAsStreamAndTranslateExceptionAsync()
                .ConfigureAwait(false);

            await foreach (var messageContent in this.GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(state, responseStream, cancellationToken).ConfigureAwait(false))
            {
                yield return messageContent;
            }
        }
    }

    private ChatCompletionState ValidateInputAndCreateChatCompletionState(
        ChatHistory chatHistory,
        Kernel? kernel,
        PromptExecutionSettings? executionSettings)
    {
        var chatHistoryCopy = new ChatHistory(chatHistory);
        ValidateAndPrepareChatHistory(chatHistoryCopy);

        var HunYuanExecutionSettings = HunYuanPromptExecutionSettings.FromExecutionSettings(executionSettings);

        return new ChatCompletionState()
        {
            AutoInvoke = false,
            ChatHistory = chatHistory,
            ExecutionSettings = HunYuanExecutionSettings,
            HunYuanRequest = this.CreateRequest(chatHistoryCopy, HunYuanExecutionSettings),
            Kernel = kernel! // not null if auto-invoke is true
        };
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(
        ChatCompletionState state,
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var chatResponsesEnumerable = this.ProcessChatResponseStreamAsync(responseStream, ct: ct);
        IAsyncEnumerator<HunYuanChatMessageContent> chatResponsesEnumerator = null!;
        try
        {
            chatResponsesEnumerator = chatResponsesEnumerable.GetAsyncEnumerator(ct);
            while (await chatResponsesEnumerator.MoveNextAsync().ConfigureAwait(false))
            {
                var messageContent = chatResponsesEnumerator.Current;

                // We disable auto-invoke because the first message in the stream doesn't contain ToolCalls or auto-invoke is already false
                state.AutoInvoke = false;

                // If we don't want to attempt to invoke any functions, just return the result.
                yield return this.GetStreamingChatContentFromChatContent(messageContent);
            }
        }
        finally
        {
            if (chatResponsesEnumerator != null)
            {
                await chatResponsesEnumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task<HunYuanChatResponse> SendRequestAndReturnValidHunYuanResponseAsync(
        Uri endpoint,
        HunYuanChatRequest request,
        CancellationToken cancellationToken)
    {
        using var httpRequestMessage = this.CreateHttpRequest(request, endpoint, this._secretKey, this._secretId);
        string body = await this.SendRequestAndGetStringBodyAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var response = DeserializeResponse<HunYuanChatResponse>(body);
        ValidateHunYuanResponse(response.Response!);
        return response;
    }

    private static void ValidateAndPrepareChatHistory(ChatHistory chatHistory)
    {
        Verify.NotNullOrEmpty(chatHistory);

        if (chatHistory.Where(message => message.Role == AuthorRole.System).ToList() is { Count: > 0 } systemMessages)
        {
            if (chatHistory.Count == systemMessages.Count)
            {
                throw new InvalidOperationException("Chat history can't contain only system messages.");
            }

            if (systemMessages.Count > 1)
            {
                throw new InvalidOperationException("Chat history can't contain more than one system message. " +
                                                    "Only the first system message will be processed but will be converted to the user message before sending to the HunYuan api.");
            }
        }
    }

    private async IAsyncEnumerable<HunYuanChatMessageContent> ProcessChatResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var response in this.ParseResponseStreamAsync(responseStream, ct: ct).ConfigureAwait(false))
        {
            foreach (var messageContent in this.ProcessChatResponse(response.Response!))
            {
                yield return messageContent;
            }
        }
    }

    private async IAsyncEnumerable<HunYuanChatResponse> ParseResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var json in this._streamJsonParser.ParseAsync(responseStream, cancellationToken: ct).ConfigureAwait(false))
        {
            yield return DeserializeResponse<HunYuanChatResponse>(json);
        }
    }

    private List<HunYuanChatMessageContent> ProcessChatResponse(HunYuanChatResponse.HunYuanMessageResponse response)
    {
        ValidateHunYuanResponse(response);

        var chatMessageContents = this.GetChatMessageContentsFromResponse(response);
        this.LogUsage(chatMessageContents);
        return chatMessageContents;
    }

    private static void ValidateHunYuanResponse(HunYuanChatResponse.HunYuanMessageResponse response)
    {
        if (response.Choices?.Count > 0)
        {
            var firstChoice = response.Choices[0];
            if (firstChoice.FinishReason == HunYuanFinishReason.Sensitive)
            {
                // TODO: Currently SK doesn't support prompt feedback/finish status, so we just throw an exception. I told SK team that we need to support it: https://github.com/microsoft/semantic-kernel/issues/4621
                throw new KernelException("Prompt was blocked due to HunYuan API safety reasons.");
            }

            if (string.IsNullOrEmpty(firstChoice.Message?.Content) && string.IsNullOrEmpty(firstChoice.Delta?.Content))
            {
                throw new KernelException("HunYuan API doesn't return any data.");
            }
        }
        else
        {
            if (response.ErrorMessage != null)
            {
                throw new KernelException($"HunYuan API returned an error: {response.ErrorMessage.Message}");
            }

            throw new KernelException("HunYuan API doesn't return any data.");
        }
    }

    private void LogUsage(List<HunYuanChatMessageContent> chatMessageContents)
        => this.LogUsageMetadata(chatMessageContents[0].Metadata!);

    private List<HunYuanChatMessageContent> GetChatMessageContentsFromResponse(HunYuanChatResponse.HunYuanMessageResponse response)
        => [this.GetChatMessageContentFromCandidate(response)];

    private HunYuanChatMessageContent GetChatMessageContentFromCandidate(HunYuanChatResponse.HunYuanMessageResponse response)
    {
        var firstChoice = response.Choices![0];
        var content = firstChoice.Message?.Content ?? string.Empty;
        return new HunYuanChatMessageContent(
            role: AuthorRole.Assistant,
            content: content,
            modelId: this._modelId,
            metadata: GetResponseMetadata(response!));
    }

    private HunYuanChatRequest CreateRequest(
        ChatHistory chatHistory,
        HunYuanPromptExecutionSettings executionSettings)
    {
        var request = HunYuanChatRequest.FromChatHistoryAndExecutionSettings(chatHistory, executionSettings);
        request.Model = this._modelId;
        return request;
    }

    private HunYuanStreamingChatMessageContent GetStreamingChatContentFromChatContent(HunYuanChatMessageContent message)
    {
        return new HunYuanStreamingChatMessageContent(
            role: message.Role,
            content: message.Content,
            modelId: this._modelId,
            metadata: message.Metadata);
    }

    private static HunYuanMetadata GetResponseMetadata(
        HunYuanChatResponse.HunYuanMessageResponse response) => new()
        {
            FinishReason = response.Choices?.FirstOrDefault()?.FinishReason,
            TotalTokenCount = response.Usage?.TotalTokens ?? 0,
            PromptTokenCount = response.Usage?.PromptTokens ?? 0,
            CompletionTokenCount = response.Usage?.CompletionTokens ?? 0
        };

    private void LogUsageMetadata(HunYuanMetadata metadata)
    {
        if (metadata.TotalTokenCount <= 0)
        {
            this.Log(LogLevel.Debug, "HunYuan usage information is not available.");
            return;
        }

        this.Log(
            LogLevel.Debug,
            "HunYuan usage metadata: Completion tokens: {CompletionTokens}, Prompt tokens: {PromptTokens}, Total tokens: {TotalTokens}",
            metadata.CompletionTokenCount,
            metadata.PromptTokenCount,
            metadata.TotalTokenCount);

        s_promptTokensCounter.Add(metadata.PromptTokenCount);
        s_completionTokensCounter.Add(metadata.CompletionTokenCount);
        s_totalTokensCounter.Add(metadata.TotalTokenCount);
    }

    private sealed class ChatCompletionState
    {
        internal ChatHistory ChatHistory { get; set; } = null!;
        internal HunYuanChatRequest HunYuanRequest { get; set; } = null!;
        internal Kernel Kernel { get; set; } = null!;
        internal HunYuanPromptExecutionSettings ExecutionSettings { get; set; } = null!;
        internal HunYuanChatMessageContent? LastMessage { get; set; }
        internal int Iteration { get; set; }
        internal bool AutoInvoke { get; set; }

        internal void AddLastMessageToChatHistoryAndRequest()
        {
            Verify.NotNull(this.LastMessage);
            this.ChatHistory.Add(this.LastMessage);
            this.HunYuanRequest.AddChatMessage(this.LastMessage);
        }
    }
}
