// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Anthropic.Core;

internal sealed class AnthropicRequest
{
    /// <summary>
    /// Model id.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = null!;

    [JsonPropertyName("messages")]
    public IList<AnthropicContent> Messages { get; set; } = null!;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("stop_sequences")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<string>? StopSequences { get; set; }

    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; set; }

    [JsonPropertyName("system")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? System { get; set; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_k")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TopK { get; set; }

    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TopP { get; set; }

    /// <summary>
    /// Creates a <see cref="AnthropicRequest"/> object from the given <see cref="ChatHistory"/> and <see cref="AnthropicPromptExecutionSettings"/>.
    /// </summary>
    /// <param name="chatHistory">The chat history to be assigned to the AnthropicRequest.</param>
    /// <param name="executionSettings">The execution settings to be applied to the AnthropicRequest.</param>
    /// <returns>A new instance of <see cref="AnthropicRequest"/>.</returns>
    public static AnthropicRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        AnthropicPromptExecutionSettings executionSettings)
    {
        AnthropicRequest obj = CreateAnthropicRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static AnthropicRequest CreateAnthropicRequest(ChatHistory chatHistory)
    {
        string? system = chatHistory.FirstOrDefault(p => p.Role == AuthorRole.System)?.Content;
        AnthropicRequest obj = new()
        {
            Messages = chatHistory.Select(CreateAnthropicContentFromChatMessage).ToList()
        };

        if (!string.IsNullOrEmpty(system))
        {
            obj.System = system;
        }

        return obj;
    }

    private static AnthropicContent CreateAnthropicContentFromChatMessage(ChatMessageContent message)
    {
        return new AnthropicContent
        {
            Content = CreateAnthropicParts(message),
            Role = message.Role
        };
    }

    private static List<AnthropicPart> CreateAnthropicParts(ChatMessageContent content)
    {
        List<AnthropicPart> parts = [];
        switch (content)
        {
            default:
                parts.AddRange(content.Items.Select(GetAnthropicPartFromKernelContent));
                break;
        }

        if (parts.Count == 0)
        {
            parts.Add(new AnthropicPart { Text = content.Content ?? string.Empty });
        }

        return parts;
    }

    private static AnthropicPart GetAnthropicPartFromKernelContent(KernelContent item) => item switch
    {
        TextContent textContent => new AnthropicPart { Text = textContent.Text },
        _ => throw new NotSupportedException($"Unsupported content type. {item.GetType().Name} is not supported by Anthropic.")
    };

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Messages);
        Verify.NotNull(message);

        this.Messages.Add(CreateAnthropicContentFromChatMessage(message));
    }

    private static void AddConfiguration(AnthropicPromptExecutionSettings executionSettings, AnthropicRequest request)
    {
        request.Temperature = executionSettings.Temperature;
        request.TopK = executionSettings.TopK;
        request.TopP = executionSettings.TopP;
        request.StopSequences = executionSettings.StopSequences;
        request.Stream = executionSettings.Stream;
    }
}
