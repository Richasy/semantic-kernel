// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Translators.Volcano.Core;

internal sealed class TextTranslateResponse
{
    public IList<TranslationItem>? TranslationList { get; set; }

    internal sealed class TranslationItem
    {
        public string? Translation { get; set; }

        public string? DetectedSourceLanguage { get; set; }
    }
}
