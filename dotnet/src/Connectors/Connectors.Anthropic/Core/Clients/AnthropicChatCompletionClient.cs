// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
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

namespace Microsoft.SemanticKernel.Connectors.Anthropic.Core;

/// <summary>
/// Represents a client for interacting with the chat completion Anthropic model.
/// </summary>
internal sealed class AnthropicChatCompletionClient : ClientBase
{
    private const string DefaultBaseUrl = "https://api.anthropic.com/v1";
    private readonly StreamJsonParser _streamJsonParser = new();
    private readonly string _modelId;
    private readonly Uri _chatGenerationEndpoint;

    /// <summary>
    /// Represents a client for interacting with the chat completion Anthropic model via GoogleAI.
    /// </summary>
    /// <param name="httpClient">HttpClient instance used to send HTTP requests</param>
    /// <param name="modelId">Id of the model supporting chat completion</param>
    /// <param name="apiKey">Api key for GoogleAI endpoint</param>
    /// <param name="baseUrl">Base url.</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public AnthropicChatCompletionClient(
        HttpClient httpClient,
        string modelId,
        string apiKey,
        Uri? baseUrl = null,
        ILogger? logger = null)
        : base(
            httpClient: httpClient,
            apiKey: apiKey,
            logger: logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(apiKey);

        this._modelId = modelId;
        baseUrl ??= new Uri(DefaultBaseUrl);
        this._chatGenerationEndpoint = new Uri($"{baseUrl.AbsoluteUri.TrimEnd('/')}/messages");
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
        var anthropicResponse = await this.SendRequestAndReturnValidAnthropicResponseAsync(
                    this._chatGenerationEndpoint, state.AnthropicRequest, cancellationToken)
                .ConfigureAwait(false);

        var chatResponses = this.ProcessChatResponse(anthropicResponse);
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
        state.AnthropicRequest.Stream = true;
        using var httpRequestMessage = this.CreateHttpRequest(state.AnthropicRequest, this._chatGenerationEndpoint, JsonGenContext.Default.AnthropicRequest);
        using var response = await this.SendRequestAndGetResponseImmediatelyAfterHeadersReadAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAndTranslateExceptionAsync()
            .ConfigureAwait(false);

        await foreach (var messageContent in this.GetStreamingChatMessageContentsAsync(state, responseStream, cancellationToken).ConfigureAwait(false))
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

        var anthropicExecutionSettings = AnthropicPromptExecutionSettings.FromExecutionSettings(executionSettings);
        ValidateMaxTokens(anthropicExecutionSettings.MaxTokens);

        return new ChatCompletionState()
        {
            AutoInvoke = false,
            ChatHistory = chatHistory,
            ExecutionSettings = anthropicExecutionSettings,
            AnthropicRequest = CreateRequest(chatHistoryCopy, anthropicExecutionSettings, kernel),
            Kernel = kernel! // not null if auto-invoke is true
        };
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatCompletionState state,
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var chatResponsesEnumerable = this.ProcessChatResponseStreamAsync(responseStream, ct: ct);
        IAsyncEnumerator<AnthropicChatMessageContent> chatResponsesEnumerator = null!;
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

    private async Task<AnthropicResponse> SendRequestAndReturnValidAnthropicResponseAsync(
        Uri endpoint,
        AnthropicRequest anthropicRequest,
        CancellationToken cancellationToken)
    {
        using var httpRequestMessage = this.CreateHttpRequest(anthropicRequest, endpoint, JsonGenContext.Default.AnthropicRequest);
        string body = await this.SendRequestAndGetStringBodyAsync(httpRequestMessage, cancellationToken)
            .ConfigureAwait(false);
        var anthropicResponse = DeserializeResponse(body, JsonGenContext.Default.AnthropicResponse);
        ValidateAnthropicResponse(anthropicResponse);
        return anthropicResponse;
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
                                                    "Only the first system message will be processed but will be converted to the user message before sending to the Anthropic api.");
            }

            chatHistory.Remove(systemMessages[0]);
        }
    }

    private async IAsyncEnumerable<AnthropicChatMessageContent> ProcessChatResponseStreamAsync(
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

    private async IAsyncEnumerable<AnthropicStreamResponse> ParseResponseStreamAsync(
        Stream responseStream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var json in this._streamJsonParser.ParseAsync(responseStream, cancellationToken: ct).ConfigureAwait(false))
        {
            yield return DeserializeResponse(json, JsonGenContext.Default.AnthropicStreamResponse);
        }
    }

    private List<AnthropicChatMessageContent> ProcessChatResponse(AnthropicResponse anthropicResponse)
    {
        ValidateAnthropicResponse(anthropicResponse);

        var chatMessageContents = this.GetChatMessageContentsFromResponse(anthropicResponse);
        return chatMessageContents;
    }

    private List<AnthropicChatMessageContent> ProcessChatResponse(AnthropicStreamResponse anthropicStreamResponse)
    {
        if (anthropicStreamResponse.Message != null)
        {
            return new List<AnthropicChatMessageContent> { this.GetChatMessageContentFromContent(anthropicStreamResponse.Message) };
        }

        if (anthropicStreamResponse.Delta != null)
        {
            var msg = new AnthropicChatMessageContent(
            role: AuthorRole.Assistant,
            content: anthropicStreamResponse.Delta.Text ?? string.Empty,
            modelId: this._modelId);
            return new List<AnthropicChatMessageContent> { msg };
        }

        return new List<AnthropicChatMessageContent>() { new(
            role: AuthorRole.Assistant,
            content: string.Empty,
            modelId: this._modelId)
        };
    }

    private static void ValidateAnthropicResponse(AnthropicResponse anthropicResponse)
    {
        if (anthropicResponse.Content == null || anthropicResponse.Content.Count == 0)
        {
            if (anthropicResponse.StopReason != AnthropicStopReason.EndReturn)
            {
                // TODO: Currently SK doesn't support prompt feedback/finish status, so we just throw an exception. I told SK team that we need to support it: https://github.com/microsoft/semantic-kernel/issues/4621
                throw new KernelException("Prompt was truncted due to Anthropic API reasons.");
            }

            throw new KernelException("Anthropic API doesn't return any data.");
        }
    }

    private List<AnthropicChatMessageContent> GetChatMessageContentsFromResponse(AnthropicResponse anthropicResponse)
        => anthropicResponse.Content!.Select(candidate => this.GetChatMessageContentFromContent(anthropicResponse)).ToList();

    private AnthropicChatMessageContent GetChatMessageContentFromContent(AnthropicResponse anthropicResponse)
    {
        AnthropicResponse.AnthropicResponseContent? part = anthropicResponse.Content?[0];
        return new AnthropicChatMessageContent(
            role: AuthorRole.Assistant,
            content: part?.Text ?? string.Empty,
            modelId: this._modelId,
            metadata: GetResponseMetadata(anthropicResponse));
    }

    private static AnthropicRequest CreateRequest(
        ChatHistory chatHistory,
        AnthropicPromptExecutionSettings anthropicExecutionSettings,
        Kernel? kernel)
    {
        var anthropicRequest = AnthropicRequest.FromChatHistoryAndExecutionSettings(chatHistory, anthropicExecutionSettings);
        return anthropicRequest;
    }

    private AnthropicStreamingChatMessageContent GetStreamingChatContentFromChatContent(AnthropicChatMessageContent message)
    {
        return new AnthropicStreamingChatMessageContent(
            role: message.Role,
            content: message.Content,
            modelId: this._modelId,
            metadata: message.Metadata);
    }

    private static AnthropicMetadata GetResponseMetadata(
        AnthropicResponse anthropicResponse) => new()
        {
            StopReason = anthropicResponse.StopReason,
            InputTokenCount = anthropicResponse.Usage?.InputTokens ?? 0,
            OutputTokenCount = anthropicResponse.Usage?.OutputTokens ?? 0,
        };

    private sealed class ChatCompletionState
    {
        internal ChatHistory ChatHistory { get; set; } = null!;
        internal AnthropicRequest AnthropicRequest { get; set; } = null!;
        internal Kernel Kernel { get; set; } = null!;
        internal AnthropicPromptExecutionSettings ExecutionSettings { get; set; } = null!;
        internal AnthropicChatMessageContent? LastMessage { get; set; }
        internal bool AutoInvoke { get; set; }

        internal void AddLastMessageToChatHistoryAndRequest()
        {
            Verify.NotNull(this.LastMessage);
            this.ChatHistory.Add(this.LastMessage);
            this.AnthropicRequest.AddChatMessage(this.LastMessage);
        }
    }
}
