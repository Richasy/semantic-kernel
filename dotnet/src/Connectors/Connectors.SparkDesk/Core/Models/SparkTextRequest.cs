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
    public SparkRequestParametersContainer? Parameter { get; set; }

    [JsonPropertyName("payload")]
    public SparkRequestPayload? Payload { get; set; }

    public void AddFunction(SparkDeskFunction function)
    {
        this.Payload ??= new SparkRequestPayload();
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
        SparkDeskPromptExecutionSettings executionSettings)
    {
        var obj = CreateSparkTextRequest(chatHistory);
        AddConfiguration(executionSettings, obj);
        return obj;
    }

    private static SparkTextRequest CreateSparkTextRequest(ChatHistory chatHistory)
    {
        SparkTextRequest obj = new()
        {
            Payload = new SparkRequestPayload
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

    private static void AddConfiguration(SparkDeskPromptExecutionSettings executionSettings, SparkTextRequest request)
    {
        request.Parameter = new SparkRequestParametersContainer { Chat = new SparkRequestParameters(executionSettings) };
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

    internal sealed class SparkRequestParametersContainer
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

        public SparkRequestParameters(SparkDeskPromptExecutionSettings settings)
        {
            this.Temperature = settings.Temperature;
            this.TopK = settings.TopK;
            this.MaxTokens = settings.MaxTokens;
            this.ChatId = settings.ChatId;
            this.Domain = settings.Version switch
            {
                SparkDeskAIVersion.V1_5 => "general",
                SparkDeskAIVersion.V2 => "generalv2",
                SparkDeskAIVersion.V3 => "generalv3",
                SparkDeskAIVersion.V3_5 => "generalv3.5",
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

    internal sealed class SparkRequestPayload
    {
        [JsonPropertyName("message")]
        public SparkMessage? Message { get; set; }

        [JsonPropertyName("functions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SparkTool? Functions { get; set; }
    }
}
