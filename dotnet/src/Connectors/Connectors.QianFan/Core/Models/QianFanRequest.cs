// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal sealed class QianFanRequest
{
    [JsonPropertyName("messages")]
    public IList<QianFanMessageContent> Messages { get; set; } = null!;

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TopP { get; set; }

    [JsonPropertyName("penalty_score")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PenaltyScore { get; set; }

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }

    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }

    [JsonPropertyName("stop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<string>? Stop { get; set; }

    [JsonPropertyName("disable_search")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DisableSearch { get; set; }

    [JsonPropertyName("enable_citation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EnableCitation { get; set; }

    [JsonPropertyName("enable_trace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EnableTrace { get; set; }

    [JsonPropertyName("max_output_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseFormat { get; set; }

    [JsonPropertyName("user_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserId { get; set; }

    /// <summary>
    /// Creates a <see cref="QianFanRequest"/> object from the given <see cref="ChatHistory"/> and <see cref="QianFanPromptExecutionSettings"/>.
    /// </summary>
    /// <param name="chatHistory">The chat history to be assigned to the QianFanRequest.</param>
    /// <param name="executionSettings">The execution settings to be applied to the QianFanRequest.</param>
    /// <returns>A new instance of <see cref="QianFanRequest"/>.</returns>
    public static QianFanRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        QianFanPromptExecutionSettings executionSettings)
    {
        QianFanRequest obj = CreateQianFanRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static QianFanRequest CreateQianFanRequest(ChatHistory chatHistory)
    {
        var systemMessage = chatHistory.FirstOrDefault(x => x.Role == AuthorRole.System);
        QianFanRequest obj = new()
        {
            Messages = chatHistory.Where(p => p.Role != AuthorRole.System).Select(CreateQianFanContentFromChatMessage).ToList()
        };

        if (systemMessage != null)
        {
            obj.System = systemMessage.Content;
        }

        return obj;
    }

    private static QianFanMessageContent CreateQianFanContentFromChatMessage(ChatMessageContent message)
    {
        return new QianFanMessageContent
        {
            Content = message.Content,
            Role = message.Role,
        };
    }

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Messages);
        Verify.NotNull(message);

        this.Messages.Add(CreateQianFanContentFromChatMessage(message));
    }

    private static void AddConfiguration(QianFanPromptExecutionSettings executionSettings, QianFanRequest request)
    {
        request.Temperature = executionSettings.Temperature;
        request.TopP = executionSettings.TopP;
        request.PenaltyScore = executionSettings.PenaltyScore;
        request.Stop = executionSettings.StopSequences;
        request.DisableSearch = executionSettings.DisableSearch;
        request.EnableCitation = executionSettings.EnableCitation;
        request.EnableTrace = executionSettings.EnableTrace;
        request.MaxOutputTokens = executionSettings.MaxTokens;
        request.ResponseFormat = executionSettings.ResponseFormat;
    }
}
