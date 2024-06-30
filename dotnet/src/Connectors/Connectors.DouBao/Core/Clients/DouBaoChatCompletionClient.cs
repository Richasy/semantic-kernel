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

namespace Microsoft.SemanticKernel.Connectors.DouBao.Core;

/// <summary>
/// Represents a client for interacting with the chat completion DouBao model.
/// </summary>
internal sealed class DouBaoChatCompletionClient : ClientBase
{
    private readonly StreamJsonParser _streamJsonParser = new();
    private readonly string _modelId;
    private readonly string _token;
    private readonly Uri _chatGenerationEndpoint;
    private static readonly string s_namespace = typeof(DouBaoChatCompletionClient).Namespace!;

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
    /// Represents a client for interacting with the chat completion DouBao model via BaiduAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="token">Api token.</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public DouBaoChatCompletionClient(
        HttpClient httpClient,
        string modelId,
        string token,
        ILogger? logger = null)
        : base(
            httpClient: httpClient,
            token: token,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(token);

        this._modelId = modelId;
        this._token = token;
        this._chatGenerationEndpoint = new Uri("https://ark.cn-beijing.volces.com/api/v3/chat/completions");
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
        var response = await this.SendRequestAndReturnValidDouBaoResponseAsync(
                    this._chatGenerationEndpoint, state.DouBaoRequest, cancellationToken)
                .ConfigureAwait(false);

        var chatResponses = this.ProcessChatResponse(response!);
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

        state.DouBaoRequest.Stream = true;
        using var httpRequestMessage = this.CreateHttpRequest(state.DouBaoRequest, this._chatGenerationEndpoint);
        using var response = await this.SendRequestAndGetResponseImmediatelyAfterHeadersReadAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAndTranslateExceptionAsync()
            .ConfigureAwait(false);

        await foreach (var messageContent in this.GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(state, responseStream, cancellationToken).ConfigureAwait(false))
        {
            yield return messageContent;
        }
    }

    private ChatCompletionState ValidateInputAndCreateChatCompletionState(
        ChatHistory chatHistory,
        Kernel? kernel,
        PromptExecutionSettings? executionSettings)
    {
        var chatHistoryCopy = new ChatHistory(chatHistory);
        ValidateAndPrepareChatHistory(chatHistoryCopy);

        var DouBaoExecutionSettings = DouBaoPromptExecutionSettings.FromExecutionSettings(executionSettings);

        return new ChatCompletionState()
        {
            AutoInvoke = false,
            ChatHistory = chatHistory,
            ExecutionSettings = DouBaoExecutionSettings,
            DouBaoRequest = this.CreateRequest(chatHistoryCopy, DouBaoExecutionSettings),
            Kernel = kernel! // not null if auto-invoke is true
        };
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(
        ChatCompletionState state,
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var chatResponsesEnumerable = this.ProcessChatResponseStreamAsync(responseStream, ct: ct);
        IAsyncEnumerator<DouBaoChatMessageContent> chatResponsesEnumerator = null!;
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

    private async Task<DouBaoChatResponse> SendRequestAndReturnValidDouBaoResponseAsync(
        Uri endpoint,
        DouBaoChatRequest request,
        CancellationToken cancellationToken)
    {
        using var httpRequestMessage = this.CreateHttpRequest(request, endpoint);
        string body = await this.SendRequestAndGetStringBodyAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var response = DeserializeResponse<DouBaoChatResponse>(body);
        ValidateDouBaoResponse(response);
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
                                                    "Only the first system message will be processed but will be converted to the user message before sending to the DouBao api.");
            }
        }
    }

    private async IAsyncEnumerable<DouBaoChatMessageContent> ProcessChatResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var response in this.ParseResponseStreamAsync(responseStream, ct: ct).ConfigureAwait(false))
        {
            foreach (var messageContent in this.ProcessChatResponse(response))
            {
                yield return messageContent;
            }
        }
    }

    private async IAsyncEnumerable<DouBaoChatResponse> ParseResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var json in this._streamJsonParser.ParseAsync(responseStream, cancellationToken: ct).ConfigureAwait(false))
        {
            yield return DeserializeResponse<DouBaoChatResponse>(json);
        }
    }

    private List<DouBaoChatMessageContent> ProcessChatResponse(DouBaoChatResponse? response)
    {
        ValidateDouBaoResponse(response);

        var chatMessageContents = this.GetChatMessageContentsFromResponse(response);
        this.LogUsage(chatMessageContents);
        return chatMessageContents;
    }

    private static void ValidateDouBaoResponse(DouBaoChatResponse? response)
    {
        if (response == null)
        {
            throw new KernelException("DouBao API returned an empty response.");
        }

        if (response.Choices?.Count > 0)
        {
            var firstChoice = response.Choices[0];
            if (firstChoice.FinishReason == DouBaoFinishReason.ContentFilter)
            {
                // TODO: Currently SK doesn't support prompt feedback/finish status, so we just throw an exception. I told SK team that we need to support it: https://github.com/microsoft/semantic-kernel/issues/4621
                throw new KernelException("Prompt was blocked due to DouBao API safety reasons.");
            }
            else if (firstChoice.FinishReason == DouBaoFinishReason.MaxLength)
            {
                throw new KernelException("Prompt was blocked due to DouBao API max tokens limit.");
            }

            if (firstChoice.FinishReason is not null && firstChoice.FinishReason != DouBaoFinishReason.Stop && string.IsNullOrEmpty(firstChoice.Message?.Content) && string.IsNullOrEmpty(firstChoice.Delta?.Content))
            {
                throw new KernelException("DouBao API doesn't return any data.");
            }
        }
        else
        {
            throw new KernelException("DouBao API doesn't return any data.");
        }
    }

    private void LogUsage(List<DouBaoChatMessageContent> chatMessageContents)
        => this.LogUsageMetadata(chatMessageContents[0].Metadata!);

    private List<DouBaoChatMessageContent> GetChatMessageContentsFromResponse(DouBaoChatResponse? response)
        => [this.GetChatMessageContentFromCandidate(response)];

    private DouBaoChatMessageContent GetChatMessageContentFromCandidate(DouBaoChatResponse? response)
    {
        if (response == null)
        {
            throw new KernelException("DouBao API returned an empty response.");
        }

        var firstChoice = response.Choices![0];
        var content = firstChoice.Message?.Content ?? firstChoice.Delta?.Content ?? string.Empty;
        return new DouBaoChatMessageContent(
            role: AuthorRole.Assistant,
            content: content,
            modelId: this._modelId,
            metadata: GetResponseMetadata(response!));
    }

    private DouBaoChatRequest CreateRequest(
        ChatHistory chatHistory,
        DouBaoPromptExecutionSettings executionSettings)
    {
        var request = DouBaoChatRequest.FromChatHistoryAndExecutionSettings(chatHistory, executionSettings);
        request.Model = this._modelId;
        return request;
    }

    private DouBaoStreamingChatMessageContent GetStreamingChatContentFromChatContent(DouBaoChatMessageContent message)
    {
        return new DouBaoStreamingChatMessageContent(
            role: message.Role,
            content: message.Content,
            modelId: this._modelId,
            metadata: message.Metadata);
    }

    private static DouBaoMetadata GetResponseMetadata(
        DouBaoChatResponse response) => new()
        {
            FinishReason = response.Choices?.FirstOrDefault()?.FinishReason,
            TotalTokenCount = response.Usage?.TotalTokens ?? 0,
            PromptTokenCount = response.Usage?.PromptTokens ?? 0,
            CompletionTokenCount = response.Usage?.CompletionTokens ?? 0
        };

    private void LogUsageMetadata(DouBaoMetadata metadata)
    {
        if (metadata.TotalTokenCount <= 0)
        {
            this.Log(LogLevel.Debug, "DouBao usage information is not available.");
            return;
        }

        this.Log(
            LogLevel.Debug,
            "DouBao usage metadata: Completion tokens: {CompletionTokens}, Prompt tokens: {PromptTokens}, Total tokens: {TotalTokens}",
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
        internal DouBaoChatRequest DouBaoRequest { get; set; } = null!;
        internal Kernel Kernel { get; set; } = null!;
        internal DouBaoPromptExecutionSettings ExecutionSettings { get; set; } = null!;
        internal DouBaoChatMessageContent? LastMessage { get; set; }
        internal int Iteration { get; set; }
        internal bool AutoInvoke { get; set; }

        internal void AddLastMessageToChatHistoryAndRequest()
        {
            Verify.NotNull(this.LastMessage);
            this.ChatHistory.Add(this.LastMessage);
            this.DouBaoRequest.AddChatMessage(this.LastMessage);
        }
    }
}
