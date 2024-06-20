// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Youdao.Core;

internal sealed class TextTranslateResponse
{
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("translation")]
    public string[]? Translation { get; set; }

    [JsonPropertyName("l")]
    public string? Language { get; set; }
}
