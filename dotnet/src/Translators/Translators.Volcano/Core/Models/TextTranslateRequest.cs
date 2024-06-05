// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Volcano.Core;

internal sealed class TextTranslateRequest
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceLanguage { get; set; }

    public string? TargetLanguage { get; set; }

    public IList<string>? TextList { get; set; }
}
