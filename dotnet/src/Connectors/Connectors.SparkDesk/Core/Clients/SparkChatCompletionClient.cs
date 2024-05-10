// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkChatCompletionClient : ClientBase
{
    private readonly SparkDeskAIVersion _version;
    private readonly string? _appId;
    private readonly Uri _chatStreamingEndpoint;

    private static readonly string s_namespace = typeof(SparkChatCompletionClient).Namespace!;

    /// <summary>
    /// The maximum number of auto-invokes that can be in-flight at any given time as part of the current
    /// asynchronous chain of execution.
    /// </summary>
    /// <remarks>
    /// This is a fail-safe mechanism. If someone accidentally manages to set up execution settings in such a way that
    /// auto-invocation is invoked recursively, and in particular where a prompt function is able to auto-invoke itself,
    /// we could end up in an infinite loop. This const is a backstop against that happening. We should never come close
    /// to this limit, but if we do, auto-invoke will be disabled for the current flow in order to prevent runaway execution.
    /// With the current setup, the way this could possibly happen is if a prompt function is configured with built-in
    /// execution settings that opt-in to auto-invocation of everything in the kernel, in which case the invocation of that
    /// prompt function could advertise itself as a candidate for auto-invocation. We don't want to outright block that,
    /// if that's something a developer has asked to do (e.g. it might be invoked with different arguments than its parent
    /// was invoked with), but we do want to limit it. This limit is arbitrary and can be tweaked in the future and/or made
    /// configurable should need arise.
    /// </remarks>
    private const int MaxInflightAutoInvokes = 5;

    /// <summary>Tracking <see cref="AsyncLocal{Int32}"/> for <see cref="MaxInflightAutoInvokes"/>.</summary>
    private static readonly AsyncLocal<int> s_inflightAutoInvokes = new();

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
    /// Represents a client for interacting with the chat completion Spark model via Spark Desk.
    /// </summary>
    /// <param name="apiKey">Api key for Spark Desk endpoint</param>
    /// <param name="secret">Secret for Spark Desk</param>
    /// <param name="appId">App ID for Spark Desk endpoint</param>
    /// <param name="apiVersion">Version of the Google API</param>
    /// <param name="logger">Logger instance used for logging (optional)</param>
    public SparkChatCompletionClient(
        string apiKey,
        string secret,
        string appId,
        SparkDeskAIVersion apiVersion,
        ILogger? logger = null)
        : base(logger: logger)
    {
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(secret);
        Verify.NotNullOrWhiteSpace(appId);

        this._appId = appId;
        this._version = apiVersion;
        string chatUrl = GetAuthorizationUrl(apiKey, secret, GetHostApi(apiVersion) + "/chat");
        this._chatStreamingEndpoint = new Uri(chatUrl);
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GenerateChatMessageAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var state = this.ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);
        var chatMessageContents = new List<ChatMessageContent>();
        await foreach (var res in this.StreamGenerateChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken).ConfigureAwait(false))
        {
            var r = res as SparkStreamingChatMessageContent;
            chatMessageContents.Add(new SparkChatMessageContent(res.Role ?? AuthorRole.Assistant, res.Content, res.ModelId ?? this._version.ToString(), r!.CalledToolResult, r.Metadata));
        }

        var contents = string.Join(string.Empty, chatMessageContents.Select(p => p.Content));
        return [new SparkChatMessageContent(AuthorRole.Assistant, contents, this._version.ToString())];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> StreamGenerateChatMessageAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = this.ValidateInputAndCreateChatCompletionState(chatHistory, kernel, executionSettings);

        for (state.Iteration = 1; ; state.Iteration++)
        {
            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(this._chatStreamingEndpoint, cancellationToken).ConfigureAwait(false);

            if (!state.AutoInvoke)
            {
                state.TextRequest.Payload!.Functions = null;
            }

            var json = JsonSerializer.Serialize(state.TextRequest);
            ArraySegment<byte> messageBuffer = new(JsonSerializer.SerializeToUtf8Bytes(state.TextRequest));
            await socket.SendAsync(messageBuffer, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);

            await foreach (var messageContent in this.GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(state, socket, cancellationToken).ConfigureAwait(false))
            {
                yield return messageContent;
            }

            if (!state.AutoInvoke)
            {
                yield break;
            }

            Verify.NotNull(state.ExecutionSettings.ToolCallBehavior);
            state.AddLastMessageToChatHistoryAndRequest();
            await this.ProcessFunctionsAsync(state, cancellationToken).ConfigureAwait(false);
            state.AutoInvoke = false;
        }
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsOrPopulateStateForToolCallingAsync(
        ChatCompletionState state,
        ClientWebSocket socket,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var chatResponsesEnumerable = this.ProcessChatResponseStreamAsync(socket, ct);
        IAsyncEnumerator<SparkTextResponse> chatResponseEnumerator = null!;
        try
        {
            chatResponseEnumerator = chatResponsesEnumerable.GetAsyncEnumerator(ct);
            while (await chatResponseEnumerator.MoveNextAsync().ConfigureAwait(false))
            {
                var messageContent = chatResponseEnumerator.Current;
                if (state.AutoInvoke && messageContent.Payload?.Choices?.Text?.FirstOrDefault()?.FunctionCall is not null)
                {
                    state.LastMessage = this.ProcessChatResponse(messageContent)[0];
                    yield break;
                }

                state.AutoInvoke = false;

                yield return this.GetStreamingChatContentFromChatContent(this.ProcessChatResponse(messageContent)[0]);
            }
        }
        finally
        {
            if (chatResponseEnumerator != null)
            {
                await chatResponseEnumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private async IAsyncEnumerable<SparkTextResponse> ProcessChatResponseStreamAsync(ClientWebSocket socket, [EnumeratorCancellation] CancellationToken ct)
    {
        var buffer = new byte[4096];
        do
        {
            var arraySegment = new ArraySegment<byte>(buffer);
            var result = await socket.ReceiveAsync(arraySegment, ct).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var stream = new MemoryStream(buffer, 0, result.Count);
                var response = JsonSerializer.Deserialize<SparkTextResponse>(stream);
                if (response?.Header?.Code != 0)
                {
                    throw new KernelException($"Spark exception: {response?.Header?.Message}");
                }

                yield return response;

                if (response.Header.Status == 2)
                {
                    break;
                }
            }
            else
            {
                throw new KernelException($"Unexpected websocket message type: {result.MessageType}");
            }
        } while (!ct.IsCancellationRequested);
    }

    private ChatCompletionState ValidateInputAndCreateChatCompletionState(
        ChatHistory chatHistory,
        Kernel? kernel,
        PromptExecutionSettings? executionSettings)
    {
        var chatHistoryCopy = new ChatHistory(chatHistory);
        ValidateAndPrepareChatHistory(chatHistoryCopy);

        var sparkExecutionSettings = SparkDeskPromptExecutionSettings.FromExecutionSettings(executionSettings);
        ValidateMaxTokens(sparkExecutionSettings.MaxTokens);

        return new ChatCompletionState
        {
            AutoInvoke = CheckAutoInvokeCondition(kernel, sparkExecutionSettings),
            ChatHistory = chatHistory,
            ExecutionSettings = sparkExecutionSettings,
            TextRequest = this.CreateRequest(chatHistoryCopy, sparkExecutionSettings, kernel),
            Kernel = kernel!
        };
    }

    private SparkTextRequest CreateRequest(
        ChatHistory chatHistory,
        SparkDeskPromptExecutionSettings sparkExecutionSettings,
        Kernel? kernel)
    {
        sparkExecutionSettings.Version = this._version;
        var sparkRequest = SparkTextRequest.FromChatHistoryAndExecutionSettings(chatHistory, sparkExecutionSettings);
        sparkRequest.Header = new SparkTextRequest.SparkRequestHeader { AppId = this._appId };
        sparkExecutionSettings.ToolCallBehavior?.ConfigureSparkTextRequest(kernel, sparkRequest);
        return sparkRequest;
    }

    private async Task ProcessFunctionsAsync(ChatCompletionState state, CancellationToken cancellationToken)
    {
        this.Log(LogLevel.Debug, "Tool requests: {Requests}", state.LastMessage!.ToolCalls!.Count);
        this.Log(LogLevel.Trace, "Function call requests: {FunctionCall}",
            string.Join(", ", state.LastMessage.ToolCalls.Select(ftc => ftc.ToString())));

        // We must send back a response for every tool call, regardless of whether we successfully executed it or not.
        // If we successfully execute it, we'll add the result. If we don't, we'll add an error.
        foreach (var toolCall in state.LastMessage.ToolCalls)
        {
            await this.ProcessSingleToolCallAsync(state, toolCall, cancellationToken).ConfigureAwait(false);
        }

        // Clear the tools. If we end up wanting to use tools, we'll reset it to the desired value.
        state.TextRequest.Payload!.Functions!.Functions = null;

        if (state.Iteration >= state.ExecutionSettings.ToolCallBehavior!.MaximumUseAttempts)
        {
            // Don't add any tools as we've reached the maximum attempts limit.
            this.Log(LogLevel.Debug, "Maximum use ({MaximumUse}) reached; removing the tools.",
                state.ExecutionSettings.ToolCallBehavior!.MaximumUseAttempts);
        }
        else
        {
            // Regenerate the tool list as necessary. The invocation of the function(s) could have augmented
            // what functions are available in the kernel.
            state.ExecutionSettings.ToolCallBehavior!.ConfigureSparkTextRequest(state.Kernel, state.TextRequest);
        }

        // Disable auto invocation if we've exceeded the allowed limit.
        if (state.Iteration >= state.ExecutionSettings.ToolCallBehavior!.MaximumAutoInvokeAttempts)
        {
            state.AutoInvoke = false;
            this.Log(LogLevel.Debug, "Maximum auto-invoke ({MaximumAutoInvoke}) reached.",
                state.ExecutionSettings.ToolCallBehavior!.MaximumAutoInvokeAttempts);
        }
    }

    private async Task ProcessSingleToolCallAsync(ChatCompletionState state, SparkFunctionToolCall toolCall, CancellationToken cancellationToken)
    {
        // Make sure the requested function is one we requested. If we're permitting any kernel function to be invoked,
        // then we don't need to check this, as it'll be handled when we look up the function in the kernel to be able
        // to invoke it. If we're permitting only a specific list of functions, though, then we need to explicitly check.
        if (state.ExecutionSettings.ToolCallBehavior?.AllowAnyRequestedKernelFunction is not true &&
            !IsRequestableTool(state.TextRequest.Payload!.Functions!.Functions!, toolCall))
        {
            this.AddToolResponseMessage(state.ChatHistory, state.TextRequest, toolCall, functionResponse: null,
                "Error: Function call request for a function that wasn't defined.");
            return;
        }

        // Ensure the provided function exists for calling
        if (!state.Kernel!.Plugins.TryGetFunctionAndArguments(toolCall, out KernelFunction? function, out KernelArguments? functionArgs))
        {
            this.AddToolResponseMessage(state.ChatHistory, state.TextRequest, toolCall, functionResponse: null,
                "Error: Requested function could not be found.");
            return;
        }

        // Now, invoke the function, and add the resulting tool call message to the chat history.
        s_inflightAutoInvokes.Value++;
        FunctionResult? functionResult;
        try
        {
            // Note that we explicitly do not use executionSettings here; those pertain to the all-up operation and not necessarily to any
            // further calls made as part of this function invocation. In particular, we must not use function calling settings naively here,
            // as the called function could in turn telling the model about itself as a possible candidate for invocation.
            functionResult = await function.InvokeAsync(state.Kernel, functionArgs, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
#pragma warning restore CA1031
        {
            this.AddToolResponseMessage(state.ChatHistory, state.TextRequest, toolCall, functionResponse: null,
                $"Error: Exception while invoking function. {e.Message}");
            return;
        }
        finally
        {
            s_inflightAutoInvokes.Value--;
        }

        this.AddToolResponseMessage(state.ChatHistory, state.TextRequest, toolCall,
            functionResponse: functionResult, errorMessage: null);
    }

    /// <summary>Checks if a tool call is for a function that was defined.</summary>
    private static bool IsRequestableTool(IEnumerable<SparkTool.FunctionDeclaration> functions, SparkFunctionToolCall ftc)
        => functions.Any(sparkFunction =>
            string.Equals(sparkFunction.Name, ftc.FullyQualifiedName, StringComparison.OrdinalIgnoreCase));

    private void AddToolResponseMessage(
        ChatHistory chat,
        SparkTextRequest request,
        SparkFunctionToolCall tool,
        FunctionResult? functionResponse,
        string? errorMessage)
    {
        if (errorMessage is not null)
        {
            this.Log(LogLevel.Debug, "Failed to handle tool request ({ToolName}). {Error}", tool.FullyQualifiedName, errorMessage);
        }

        var message = new SparkChatMessageContent(AuthorRole.Tool,
            content: errorMessage ?? string.Empty,
            modelId: this._version.ToString(),
            calledToolResult: functionResponse != null ? new(tool, functionResponse) : null,
            metadata: null);

        request.AddChatMessage(message);
        chat.Clear();
        foreach (var msg in request.Payload!.Message!.Text!)
        {
            chat.AddMessage(msg.Role!.Value, msg.Content ?? string.Empty);
        }
    }

    private List<SparkChatMessageContent> ProcessChatResponse(SparkTextResponse sparkResponse)
    {
        ValidateSparkTextResponse(sparkResponse);

        var chatMessageContents = this.GetChatMessageContentsFromResponse(sparkResponse);
        this.LogUsage(chatMessageContents);
        return chatMessageContents;
    }

    private static void ValidateSparkTextResponse(SparkTextResponse sparkResponse)
    {
        if (sparkResponse?.Payload?.Choices?.Text == null || sparkResponse?.Payload?.Choices?.Text.Count == 0)
        {
            throw new KernelException("Spark API doesn't return any data.");
        }
    }

    private void LogUsage(List<SparkChatMessageContent> chatMessageContents)
        => this.LogUsageMetadata(chatMessageContents[0].Metadata!);

    private List<SparkChatMessageContent> GetChatMessageContentsFromResponse(SparkTextResponse sparkResponse)
        => sparkResponse.Payload!.Choices!.Text!.Select(candidate => this.GetChatMessageContentFromCandidate(sparkResponse, candidate)).ToList();

    private SparkChatMessageContent GetChatMessageContentFromCandidate(SparkTextResponse sparkResponse, SparkResponseTextChoice candidate)
    {
        SparkFunctionCall[]? toolCalls = candidate?.FunctionCall is { } function ? [function] : null;
        return new SparkChatMessageContent(
            role: candidate?.Role ?? AuthorRole.Assistant,
            content: candidate?.Content ?? string.Empty,
            modelId: this._version.ToString(),
            functionsToolCalls: toolCalls,
            metadata: GetResponseMetadata(sparkResponse));
    }

    private SparkStreamingChatMessageContent GetStreamingChatContentFromChatContent(SparkChatMessageContent message)
    {
        if (message.CalledToolResult != null)
        {
            return new SparkStreamingChatMessageContent(
                role: message.Role,
                content: message.Content,
                modelId: this._version.ToString(),
                calledToolResult: message.CalledToolResult,
                metadata: message.Metadata);
        }

        if (message.ToolCalls != null)
        {
            return new SparkStreamingChatMessageContent(
                role: message.Role,
                content: message.Content,
                modelId: this._version.ToString(),
                toolCalls: message.ToolCalls,
                metadata: message.Metadata);
        }

        return new SparkStreamingChatMessageContent(
            role: message.Role,
            content: message.Content,
            modelId: this._version.ToString(),
            metadata: message.Metadata);
    }

    private static bool CheckAutoInvokeCondition(Kernel? kernel, SparkDeskPromptExecutionSettings sparkExecutionSettings)
    {
        bool autoInvoke = kernel is not null
                          && sparkExecutionSettings.ToolCallBehavior?.MaximumAutoInvokeAttempts > 0
                          && s_inflightAutoInvokes.Value < MaxInflightAutoInvokes;
        return autoInvoke;
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
                                                    "Only the first system message will be processed but will be converted to the user message before sending to the Spark api.");
            }
        }
    }

    private static SparkMetadata GetResponseMetadata(
        SparkTextResponse sparkResponse) => new()
        {
            PromptTokenCount = sparkResponse.Payload?.Usage?.Text?.PromptTokens ?? 0,
            CompletionTokenCount = sparkResponse.Payload?.Usage?.Text?.CompletionTokens ?? 0,
            TotalTokenCount = sparkResponse.Payload?.Usage?.Text?.TotalTokens ?? 0,
            QuestionTokenCount = sparkResponse.Payload?.Usage?.Text?.QuestionTokens ?? 0,
            Status = sparkResponse?.Header?.Status ?? -1,
        };

    private void LogUsageMetadata(SparkMetadata metadata)
    {
        if (metadata.TotalTokenCount <= 0)
        {
            this.Log(LogLevel.Debug, "Spark usage information is not available.");
            return;
        }

        this.Log(
            LogLevel.Debug,
            "Spark usage metadata: Question tokens: {QuestionTokens}, Completion tokens: {PromptTokens}, Total tokens: {TotalTokens}",
            metadata.QuestionTokenCount,
            metadata.PromptTokenCount,
            metadata.TotalTokenCount);

        s_promptTokensCounter.Add(metadata.PromptTokenCount);
        s_completionTokensCounter.Add(metadata.CompletionTokenCount);
        s_totalTokensCounter.Add(metadata.TotalTokenCount);
    }

    private sealed class ChatCompletionState
    {
        internal ChatHistory ChatHistory { get; set; } = null!;

        internal SparkTextRequest TextRequest { get; set; } = null!;

        internal Kernel Kernel { get; set; } = null!;

        internal SparkDeskPromptExecutionSettings ExecutionSettings { get; set; } = null!;

        internal SparkChatMessageContent? LastMessage { get; set; }

        internal int Iteration { get; set; }

        internal bool AutoInvoke { get; set; }

        internal void AddLastMessageToChatHistoryAndRequest()
        {
            Verify.NotNull(this.LastMessage);
            this.ChatHistory.Add(this.LastMessage);
            this.TextRequest.AddChatMessage(this.LastMessage);
        }
    }
}
