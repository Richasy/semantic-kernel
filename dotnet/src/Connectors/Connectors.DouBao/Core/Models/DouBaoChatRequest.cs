// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.DouBao.Core;

internal sealed class DouBaoChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public IList<DouBaoMessageContent> Messages { get; set; } = null!;

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TopP { get; set; }

    [JsonPropertyName("max_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("frequency_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FrequencyPenalty { get; set; }

    [JsonPropertyName("stop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<string>? StopSequences { get; set; }

    /// <summary>
    /// Creates a <see cref="DouBaoChatRequest"/> object from the given <see cref="ChatHistory"/> and <see cref="DouBaoPromptExecutionSettings"/>.
    /// </summary>
    /// <param name="chatHistory">The chat history to be assigned to the DouBaoRequest.</param>
    /// <param name="executionSettings">The execution settings to be applied to the DouBaoRequest.</param>
    /// <returns>A new instance of <see cref="DouBaoChatRequest"/>.</returns>
    public static DouBaoChatRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        DouBaoPromptExecutionSettings executionSettings)
    {
        DouBaoChatRequest obj = CreateDouBaoRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static DouBaoChatRequest CreateDouBaoRequest(ChatHistory chatHistory)
    {
        DouBaoChatRequest obj = new()
        {
            Messages = chatHistory.Where(p => p.Role != AuthorRole.Tool).Select(CreateDouBaoContentFromChatMessage).ToList()
        };

        return obj;
    }

    private static DouBaoMessageContent CreateDouBaoContentFromChatMessage(ChatMessageContent message)
    {
        return new DouBaoMessageContent
        {
            Content = message.Content,
            Role = message.Role,
        };
    }

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Messages);
        Verify.NotNull(message);

        this.Messages.Add(CreateDouBaoContentFromChatMessage(message));
    }

    private static void AddConfiguration(DouBaoPromptExecutionSettings executionSettings, DouBaoChatRequest request)
    {
        request.Temperature = executionSettings.Temperature;
        request.TopP = executionSettings.TopP;
        request.MaxTokens = executionSettings.MaxTokens;
        request.FrequencyPenalty = executionSettings.FrequencyPenalty;
        request.StopSequences = executionSettings.StopSequences;
    }
}
