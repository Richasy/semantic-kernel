// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.DouBao.Core;
internal sealed class DouBaoChatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("created")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("usage")]
    public DouBaoUsage? Usage { get; set; }

    [JsonPropertyName("choices")]
    public IList<DouBaoResponseChoice>? Choices { get; set; }

    internal sealed class DouBaoUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    internal sealed class DouBaoResponseChoice
    {
        [JsonPropertyName("finish_reason")]
        public DouBaoFinishReason? FinishReason { get; set; }

        [JsonPropertyName("delta")]
        public DouBaoMessageContent? Delta { get; set; }

        [JsonPropertyName("message")]
        public DouBaoMessageContent? Message { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
