// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanRequest
{
    [JsonPropertyName("Messages")]
    public IList<HunYuanMessageContent> Messages { get; set; } = null!;

    [JsonPropertyName("Temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; set; }

    [JsonPropertyName("TopP")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TopP { get; set; }

    [JsonPropertyName("Stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }

    [JsonPropertyName("StreamModeration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? StreamModeration { get; set; }

    [JsonPropertyName("Model")]
    public string Model { get; set; } = null!;

    /// <summary>
    /// Creates a <see cref="HunYuanRequest"/> object from the given <see cref="ChatHistory"/> and <see cref="HunYuanPromptExecutionSettings"/>.
    /// </summary>
    /// <param name="chatHistory">The chat history to be assigned to the HunYuanRequest.</param>
    /// <param name="executionSettings">The execution settings to be applied to the HunYuanRequest.</param>
    /// <returns>A new instance of <see cref="HunYuanRequest"/>.</returns>
    public static HunYuanRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        HunYuanPromptExecutionSettings executionSettings)
    {
        HunYuanRequest obj = CreateHunYuanRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static HunYuanRequest CreateHunYuanRequest(ChatHistory chatHistory)
    {
        HunYuanRequest obj = new()
        {
            Messages = chatHistory.Where(p => p.Role != AuthorRole.Tool).Select(CreateHunYuanContentFromChatMessage).ToList()
        };

        return obj;
    }

    private static HunYuanMessageContent CreateHunYuanContentFromChatMessage(ChatMessageContent message)
    {
        return new HunYuanMessageContent
        {
            Content = message.Content,
            Role = message.Role,
        };
    }

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Messages);
        Verify.NotNull(message);

        this.Messages.Add(CreateHunYuanContentFromChatMessage(message));
    }

    private static void AddConfiguration(HunYuanPromptExecutionSettings executionSettings, HunYuanRequest request)
    {
        request.Temperature = executionSettings.Temperature;
        request.TopP = executionSettings.TopP;
        request.StreamModeration = executionSettings.StreamModeration;
    }
}
