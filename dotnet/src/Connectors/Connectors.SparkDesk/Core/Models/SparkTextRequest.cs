// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkTextRequest
{
    [JsonPropertyName("header")]
    public SparkRequestHeader? Header { get; set; }

    [JsonPropertyName("parameter")]
    public SparkTextRequestParametersContainer? Parameter { get; set; }

    [JsonPropertyName("payload")]
    public SparkTextRequestPayload? Payload { get; set; }

    public void AddFunction(SparkDeskFunction function)
    {
        this.Payload ??= new SparkTextRequestPayload();
        this.Payload.Functions ??= new SparkTool();
        this.Payload.Functions.Functions ??= [];

        this.Payload.Functions.Functions.Add(function.ToFunctionDeclaration());
    }

    public void AddChatMessage(ChatMessageContent message)
    {
        Verify.NotNull(this.Payload?.Message?.Text);
        Verify.NotNull(message);

        if (message.Role == AuthorRole.Tool)
        {
            // 2024.05.09
            // 由于星火模型不支持 Tool 角色，所以我们需要将 Tool 角色的消息转换为用户消息并要求模型转述（实际效果并不好，还是建议别用）
            var chatList = this.Payload.Message.Text;
            chatList.RemoveAt(chatList.Count - 1);
            var sparkMessage = (SparkChatMessageContent)message;
            var toolMessage = sparkMessage.CalledToolResult!.FunctionResult.ToString();
            var userMessage = new SparkMessage.SparkTextMessage
            {
                Role = AuthorRole.User,
                Content = $"Output this: {toolMessage}",
            };

            chatList.Add(userMessage);
        }
        else if (!string.IsNullOrEmpty(message.Content))
        {
            this.Payload.Message.Text.Add(CreateSparkTextMessageFromChatMessage(message));
        }
    }

    public static SparkTextRequest FromChatHistoryAndExecutionSettings(
        ChatHistory chatHistory,
        SparkDeskPromptExecutionSettings executionSettings,
        string modelId)
    {
        var obj = CreateSparkTextRequest(chatHistory);
        AddConfiguration(executionSettings, obj, modelId);
        return obj;
    }

    private static SparkTextRequest CreateSparkTextRequest(ChatHistory chatHistory)
    {
        SparkTextRequest obj = new()
        {
            Payload = new SparkTextRequestPayload
            {
                Message = new SparkMessage
                {
                    Text = chatHistory.Select(CreateSparkTextMessageFromChatMessage).ToList(),
                }
            }
        };

        return obj;
    }

    private static SparkMessage.SparkTextMessage CreateSparkTextMessageFromChatMessage(ChatMessageContent message)
    {
        return new SparkMessage.SparkTextMessage
        {
            Role = message.Role,
            Content = message.Content,
        };
    }

    private static void AddConfiguration(SparkDeskPromptExecutionSettings executionSettings, SparkTextRequest request, string modelId)
    {
        request.Parameter = new SparkTextRequestParametersContainer { Chat = new SparkRequestParameters(executionSettings, modelId) };
    }

    internal sealed class SparkTextRequestParametersContainer
    {
        [JsonPropertyName("chat")]
        [JsonRequired]
        public SparkRequestParameters? Chat { get; set; }
    }

    internal sealed class SparkRequestParameters
    {
        public SparkRequestParameters()
        {
        }

        public SparkRequestParameters(SparkDeskPromptExecutionSettings settings, string modelId)
        {
            this.Temperature = settings.Temperature;
            this.TopK = settings.TopK;
            this.MaxTokens = settings.MaxTokens;
            this.ChatId = settings.ChatId;
            this.Domain = modelId switch
            {
                "v1.1" => "general",
                "v2.1" => "generalv2",
                "v3.1" => "generalv3",
                "v3.5" => "generalv3.5",
                "v4.0" => "4.0Ultra",
                _ => default,
            };
        }

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Temperature { get; set; }

        [JsonPropertyName("top_k")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TopK { get; set; }

        [JsonPropertyName("max_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("chat_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChatId { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }
    }

    internal sealed class SparkTextRequestPayload
    {
        [JsonPropertyName("message")]
        public SparkMessage? Message { get; set; }

        [JsonPropertyName("functions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SparkTool? Functions { get; set; }
    }
}

internal sealed class SparkRequestHeader
{
    /// <summary>
    /// The application appid, obtained from the open platform control panel.
    /// </summary>
    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    /// <summary>
    /// The user's id, used to distinguish between different users. Not required.
    /// </summary>
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
}
