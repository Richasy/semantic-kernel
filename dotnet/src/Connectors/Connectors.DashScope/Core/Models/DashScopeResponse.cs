// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.DashScope.Core;

internal sealed class DashScopeResponse
{
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("output")]
    public DashScopeResponseOutput? Output { get; set; }

    [JsonPropertyName("usage")]
    public DashScopeResponseUsage? Usage { get; set; }

    internal sealed class DashScopeResponseOutput
    {
        [JsonPropertyName("choices")]
        public IList<DashScopeResponseChoice>? Choices { get; set; }
    }

    internal sealed class DashScopeResponseUsage
    {
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }

        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
