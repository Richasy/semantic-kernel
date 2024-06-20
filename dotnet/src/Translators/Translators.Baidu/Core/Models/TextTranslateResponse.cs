// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Baidu.Core;

internal sealed class TextTranslateResponse
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("trans_result")]
    public IList<TranslationResult>? Result { get; set; }

    internal sealed class TranslationResult
    {
        [JsonPropertyName("src")]
        public string? Source { get; set; }

        [JsonPropertyName("dst")]
        public string? Result { get; set; }
    }
}
