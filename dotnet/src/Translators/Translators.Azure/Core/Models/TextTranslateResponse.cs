// Copyright (c) Richasy. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Azure.Core;

internal sealed class TextTranslateResponse
{
    [JsonPropertyName("detectedLanguage")]
    public DetectedLanguageResponse? DetectedLanguage { get; set; }

    [JsonPropertyName("translations")]
    public IList<TranslationItem>? Translations { get; set; }

    internal sealed class DetectedLanguageResponse
    {
        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    internal sealed class TranslationItem
    {
        [JsonPropertyName("to")]
        public string? To { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
