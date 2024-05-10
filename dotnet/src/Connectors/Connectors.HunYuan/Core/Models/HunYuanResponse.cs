// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanResponse
{
    [JsonPropertyName("Response")]
    public HunYuanMessageResponse? Response { get; set; }

    internal sealed class HunYuanMessageResponse
    {
        [JsonPropertyName("Created")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// 免责声明.
        /// </summary>
        [JsonPropertyName("Note")]
        public string? Note { get; set; }

        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Usage")]
        public HunYuanUsage? Usage { get; set; }

        [JsonPropertyName("Choices")]
        public IList<HunYuanResponseChoice>? Choices { get; set; }

        [JsonPropertyName("ErrorMsg")]
        public HunYuanErrorMessage? ErrorMessage { get; set; }

        internal sealed class HunYuanUsage
        {
            [JsonPropertyName("PromptTokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("CompletionTokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("TotalTokens")]
            public int TotalTokens { get; set; }
        }

        internal sealed class HunYuanResponseChoice
        {
            [JsonPropertyName("FinishReason")]
            public HunYuanFinishReason? FinishReason { get; set; }

            [JsonPropertyName("Delta")]
            public HunYuanMessageContent? Delta { get; set; }

            [JsonPropertyName("Message")]
            public HunYuanMessageContent? Message { get; set; }
        }

        internal sealed class HunYuanErrorMessage
        {
            [JsonPropertyName("Code")]
            public int Code { get; set; }

            [JsonPropertyName("Msg")]
            public string? Message { get; set; }
        }
    }
}


