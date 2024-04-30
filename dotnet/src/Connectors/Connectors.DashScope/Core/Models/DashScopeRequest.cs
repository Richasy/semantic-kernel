// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.DashScope.Core;

internal sealed class DashScopeRequest
{
    [JsonPropertyName("model")]
    [JsonRequired]
    public string? Model { get; set; }

    [JsonPropertyName("input")]
    [JsonRequired]
    public DashScopeRequestInput Input { get; set; } = null!;

    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DashScopeRequestParameters? Parameters { get; set; }

    public void AddFunction(DashScopeFunction function)
    {
        this.Parameters ??= new DashScopeRequestParameters();
        this.Parameters.Tools ??= [];
        this.Parameters.Tools.Add(function.ToFunctionDeclaration());
    }

    /// <summary>
    /// Creates a <see cref="DashScopeRequest"/> object from the given <see cref="ChatHistory"/> and <see cref="DashScopePromptExecutionSettings"/>.
    /// </summary>
    /// <param name="chatHistory">The chat history to be assigned to the DashScopeRequest.</param>
    /// <param name="executionSettings">The execution settings to be applied to the DashScopeRequest.</param>
    /// <returns>A new instance of <see cref="DashScopeRequest"/>.</returns>
    public static DashScopeRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        DashScopePromptExecutionSettings executionSettings)
    {
        DashScopeRequest obj = CreateDashScopeRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static DashScopeRequest CreateDashScopeRequest(ChatHistory chatHistory)
    {
        DashScopeRequest obj = new()
        {
            Input = new DashScopeRequestInput { Messages = chatHistory.Select(CreateDashScopeContentFromChatMessage).ToList() },
        };
        return obj;
    }

    private static DashScopeContent CreateDashScopeContentFromChatMessage(ChatMessageContent message)
    {
        return new DashScopeContent
        {
            Content = message.Content,
            Role = message.Role
        };
    }

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Input.Messages);
        Verify.NotNull(message);

        this.Input.Messages.Add(CreateDashScopeContentFromChatMessage(message));
    }

    private static void AddConfiguration(DashScopePromptExecutionSettings executionSettings, DashScopeRequest request)
    {
        request.Parameters = new DashScopeRequestParameters
        {
            Temperature = executionSettings.Temperature,
            TopP = executionSettings.TopP,
            TopK = executionSettings.TopK,
            RepetitionPenalty = executionSettings.RepetitionPenalty,
            Stop = executionSettings.StopSequences,
            EnableSearch = executionSettings.EnableSearch,
            Seed = executionSettings.Seed,
            MaxTokens = executionSettings.MaxTokens,
            ResultFormat = executionSettings.ResultFormat,
        };
    }

    internal sealed class DashScopeRequestInput
    {
        [JsonPropertyName("prompt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prompt { get; set; }

        [JsonPropertyName("messages")]
        [JsonRequired]
        public IList<DashScopeContent> Messages { get; set; } = null!;
    }

    internal sealed class DashScopeRequestParameters
    {
        [JsonPropertyName("result_format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ResultFormat { get; set; }

        [JsonPropertyName("seed")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Seed { get; set; }

        [JsonPropertyName("max_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Temperature { get; set; }

        [JsonPropertyName("top_p")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TopP { get; set; }

        [JsonPropertyName("top_k")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TopK { get; set; }

        [JsonPropertyName("repetition_penalty")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? RepetitionPenalty { get; set; }

        [JsonPropertyName("stop")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? Stop { get; set; }

        [JsonPropertyName("enable_search")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableSearch { get; set; }

        [JsonPropertyName("incremental_output")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IncrementalOutput { get; set; }

        [JsonPropertyName("tools")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<DashScopeTool>? Tools { get; set; }
    }
}
