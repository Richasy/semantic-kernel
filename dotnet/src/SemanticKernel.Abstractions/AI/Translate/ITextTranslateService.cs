// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Translate;

/// <summary>
/// 文本翻译服务.
/// </summary>
public interface ITextTranslateService : IAIService
{
    /// <summary>
    /// 获取文本翻译结果.
    /// </summary>
    /// <param name="text">输入的文本.</param>
    /// <param name="settings">请求设置.</param>
    /// <param name="cancellationToken">终止令牌.</param>
    /// <returns>翻译结果.</returns>
    Task<IReadOnlyList<TranslateTextContent>> GetTextTranslateResultAsync(
        string text,
        TranslateExecutionSettings? settings = null,
        CancellationToken? cancellationToken = default);
}
