// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Tencent.Core;

internal sealed class TextTranslateRequest
{
    public string? SourceText { get; set; }

    public string? Source { get; set; }

    public string? Target { get; set; }

    public int ProjectId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UntranslatedText { get; set; }
}
