// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel;

/// <summary>
/// 文本翻译结果.
/// </summary>
public sealed class TranslateTextContent
{
    /// <summary>
    /// 翻译结果.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 源语言.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 目标语言.
    /// </summary>
    public string? Target { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => this.Text ?? string.Empty;
}
