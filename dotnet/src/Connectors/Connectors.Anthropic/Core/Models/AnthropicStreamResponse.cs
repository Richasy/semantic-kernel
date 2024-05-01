// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.Anthropic.Core;

internal sealed class AnthropicStreamResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("message")]
    public AnthropicResponse? Message { get; set; }

    [JsonPropertyName("content_block")]
    public AnthropicResponse.AnthropicResponseContent? ContentBlock { get; set; }

    [JsonPropertyName("delta")]
    public AnthropicResponse.AnthropicResponseContent? Delta { get; set; }

    [JsonPropertyName("usage")]
    public AnthropicResponse.AnthropicResponseUsage? Usage { get; set; }
}
