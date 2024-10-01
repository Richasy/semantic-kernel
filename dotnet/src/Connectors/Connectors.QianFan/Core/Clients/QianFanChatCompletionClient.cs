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

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

/// <summary>
/// Represents a client for interacting with the chat completion QianFan model.
/// </summary>
internal sealed class QianFanChatCompletionClient : ClientBase
{
    private readonly StreamJsonParser _streamJsonParser = new();
    private readonly string _modelId;
    private readonly Uri _chatGenerationEndpoint;

    private static readonly string s_namespace = typeof(QianFanChatCompletionClient).Namespace!;

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
    /// Represents a client for interacting with the chat completion QianFan model via BaiduAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="apiKey">Api key for BaiduAI endpoint</param>
    /// <param name="apiSecret">Api secret for BaiduAI</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public QianFanChatCompletionClient(
        HttpClient httpClient,
        string modelId,
        string apiKey,
        string apiSecret,
        ILogger? logger = null)
        : base(
            apiKey,
            apiSecret,
            httpClient: httpClient,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);

        this._modelId = modelId;
        this._chatGenerationEndpoint = new Uri($"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{this._modelId}");
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
        var state = ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);
        var qianFanResponse = await this.SendRequestAndReturnValidQianFanResponseAsync(
                    this._chatGenerationEndpoint, state.QianFanRequest, cancellationToken)
                .ConfigureAwait(false);

        var chatResponses = this.ProcessChatResponse(qianFanResponse);
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
        var state = ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);

        state.QianFanRequest.Stream = true;
        await this.EnsureAuthTokenAsync().ConfigureAwait(false);
        using var httpRequestMessage = this.CreateHttpRequest(state.QianFanRequest, JsonGenContext.Default.QianFanChatRequest, this._chatGenerationEndpoint);
        using var response = await this.SendRequestAndGetResponseImmediatelyAfterHeadersReadAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAndTranslateExceptionAsync()
            .ConfigureAwait(false);

        await foreach (var messageContent in this.GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(state, responseStream, cancellationToken).ConfigureAwait(false))
        {
            yield return messageContent;
        }
    }

    private static ChatCompletionState ValidateInputAndCreateChatCompletionState(
        ChatHistory chatHistory,
        Kernel? kernel,
        PromptExecutionSettings? executionSettings)
    {
        var chatHistoryCopy = new ChatHistory(chatHistory);
        ValidateAndPrepareChatHistory(chatHistoryCopy);

        var qianFanExecutionSettings = QianFanPromptExecutionSettings.FromExecutionSettings(executionSettings);
        ValidateMaxTokens(qianFanExecutionSettings.MaxTokens);

        return new ChatCompletionState()
        {
            AutoInvoke = false,
            ChatHistory = chatHistory,
            ExecutionSettings = qianFanExecutionSettings,
            QianFanRequest = CreateRequest(chatHistoryCopy, qianFanExecutionSettings, kernel),
            Kernel = kernel! // not null if auto-invoke is true
        };
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(
        ChatCompletionState state,
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var chatResponsesEnumerable = this.ProcessChatResponseStreamAsync(responseStream, ct: ct);
        IAsyncEnumerator<QianFanChatMessageContent> chatResponsesEnumerator = null!;
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

    private async Task<QianFanChatResponse> SendRequestAndReturnValidQianFanResponseAsync(
        Uri endpoint,
        QianFanChatRequest qianFanRequest,
        CancellationToken cancellationToken)
    {
        await this.EnsureAuthTokenAsync().ConfigureAwait(false);
        using var httpRequestMessage = this.CreateHttpRequest(qianFanRequest, JsonGenContext.Default.QianFanChatRequest, endpoint);
        string body = await this.SendRequestAndGetStringBodyAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var qianFanResponse = DeserializeResponse<QianFanChatResponse>(body, JsonGenContext.Default.QianFanChatResponse);
        ValidateQianFanResponse(qianFanResponse);
        return qianFanResponse;
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
                                                    "Only the first system message will be processed but will be converted to the user message before sending to the QianFan api.");
            }

            chatHistory.Remove(systemMessages.First());
        }
    }

    private async IAsyncEnumerable<QianFanChatMessageContent> ProcessChatResponseStreamAsync(
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

    private async IAsyncEnumerable<QianFanChatResponse> ParseResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var json in this._streamJsonParser.ParseAsync(responseStream, cancellationToken: ct).ConfigureAwait(false))
        {
            yield return DeserializeResponse<QianFanChatResponse>(json, JsonGenContext.Default.QianFanChatResponse);
        }
    }

    private List<QianFanChatMessageContent> ProcessChatResponse(QianFanChatResponse qianFanResponse)
    {
        ValidateQianFanResponse(qianFanResponse);

        var chatMessageContents = this.GetChatMessageContentsFromResponse(qianFanResponse);
        this.LogUsage(chatMessageContents);
        return chatMessageContents;
    }

    private static void ValidateQianFanResponse(QianFanChatResponse qianFanResponse)
    {
        if (string.IsNullOrEmpty(qianFanResponse.Result))
        {
            if (qianFanResponse.FinishReason == QianFanFinishReason.Filter)
            {
                // TODO: Currently SK doesn't support prompt feedback/finish status, so we just throw an exception. I told SK team that we need to support it: https://github.com/microsoft/semantic-kernel/issues/4621
                throw new KernelException("Prompt was blocked due to QianFan API safety reasons.");
            }

            if (qianFanResponse.FinishReason != QianFanFinishReason.Normal && ((qianFanResponse.IsEnd && string.IsNullOrEmpty(qianFanResponse.Result)) || (qianFanResponse.Object == null)))
            {
                throw new KernelException("QianFan API doesn't return any data.");
            }
        }
    }

    private void LogUsage(List<QianFanChatMessageContent> chatMessageContents)
        => this.LogUsageMetadata(chatMessageContents[0].Metadata!);

    private List<QianFanChatMessageContent> GetChatMessageContentsFromResponse(QianFanChatResponse qianFanResponse)
        => [this.GetChatMessageContentFromCandidate(qianFanResponse)];

    private QianFanChatMessageContent GetChatMessageContentFromCandidate(QianFanChatResponse qianFanResponse)
    {
        return new QianFanChatMessageContent(
            role: AuthorRole.Assistant,
            content: qianFanResponse?.Result ?? string.Empty,
            modelId: this._modelId,
            metadata: GetResponseMetadata(qianFanResponse!));
    }

    private static QianFanChatRequest CreateRequest(
        ChatHistory chatHistory,
        QianFanPromptExecutionSettings qianFanExecutionSettings,
        Kernel? kernel)
    {
        var qianFanRequest = QianFanChatRequest.FromChatHistoryAndExecutionSettings(chatHistory, qianFanExecutionSettings);
        return qianFanRequest;
    }

    private QianFanStreamingChatMessageContent GetStreamingChatContentFromChatContent(QianFanChatMessageContent message)
    {
        return new QianFanStreamingChatMessageContent(
            role: message.Role,
            content: message.Content,
            modelId: this._modelId,
            choiceIndex: message.Metadata!.SentenceId,
            metadata: message.Metadata);
    }

    private static QianFanMetadata GetResponseMetadata(
        QianFanChatResponse qianFanResponse) => new()
        {
            FinishReason = qianFanResponse.FinishReason,
            IsEnd = qianFanResponse.IsEnd,
            IsTruncated = qianFanResponse.IsTruncated,
            SentenceId = qianFanResponse.SentenceId,
            CreatedAt = qianFanResponse.CreatedAt,
            NeedClearHistory = qianFanResponse.NeedClearHistory,
            Flag = qianFanResponse.Flag,
            BanRound = qianFanResponse.BanRound,
            TotalTokenCount = qianFanResponse.Usage?.TotalTokens ?? 0,
            PromptTokenCount = qianFanResponse.Usage?.PromptTokens ?? 0,
            CompletionTokenCount = qianFanResponse.Usage?.CompletionTokens ?? 0
        };

    private void LogUsageMetadata(QianFanMetadata metadata)
    {
        if (metadata.TotalTokenCount <= 0)
        {
            this.Log(LogLevel.Debug, "QianFan usage information is not available.");
            return;
        }

        this.Log(
            LogLevel.Debug,
            "QianFan usage metadata: Completion tokens: {CompletionTokens}, Prompt tokens: {PromptTokens}, Total tokens: {TotalTokens}",
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
        internal QianFanChatRequest QianFanRequest { get; set; } = null!;
        internal Kernel Kernel { get; set; } = null!;
        internal QianFanPromptExecutionSettings ExecutionSettings { get; set; } = null!;
        internal QianFanChatMessageContent? LastMessage { get; set; }
        internal int Iteration { get; set; }
        internal bool AutoInvoke { get; set; }

        internal void AddLastMessageToChatHistoryAndRequest()
        {
            Verify.NotNull(this.LastMessage);
            this.ChatHistory.Add(this.LastMessage);
            this.QianFanRequest.AddChatMessage(this.LastMessage);
        }
    }
}
