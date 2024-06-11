// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal sealed class QianFanChatResponse
{
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("sentence_id")]
    public int SentenceId { get; set; }

    [JsonPropertyName("is_end")]
    public bool IsEnd { get; set; }

    [JsonPropertyName("is_truncated")]
    public bool IsTruncated { get; set; }

    [JsonPropertyName("finish_reason")]
    public QianFanFinishReason FinishReason { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("need_clear_history")]
    public bool NeedClearHistory { get; set; }

    [JsonPropertyName("flag")]
    public int Flag { get; set; }

    [JsonPropertyName("ban_round")]
    public int BanRound { get; set; }

    [JsonPropertyName("usage")]
    public QianFanUsage? Usage { get; set; }

    internal sealed class QianFanUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
