// Copyright (c) Microsoft. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Translators.Azure.Core;

internal sealed class TextTranslateRequest : IEnumerable<TextTranslateRequest.TextItem>
{
    private readonly List<TextItem> _textItems = new();

    public IEnumerator<TextItem> GetEnumerator() => this._textItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public void AddText(string text)
        => this._textItems.Add(new TextItem() { Text = text });

    internal sealed class TextItem
    {
        public string Text { get; set; } = string.Empty;
    }
}
