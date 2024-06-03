// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel.Translators.Tencent.Core;

internal sealed class TextTranslateResponse
{
    public TextTranslateResponseContent? Response { get; set; }

    internal class TextTranslateResponseContent
    {
        public string? TargetText { get; set; }

        public string? Source { get; set; }

        public string? Target { get; set; }

        public string? RequestId { get; set; }
    }
}
