// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Tencent.Core;

namespace Microsoft.SemanticKernel.Translators.Tencent;

/// <summary>
/// Tencent 文本翻译服务.
/// </summary>
public sealed class TencentTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly TencentTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TencentTextTranslateService"/> class.
    /// </summary>
    public TencentTextTranslateService(
        string secretId,
        string secretKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(secretId);
        Verify.NotNullOrWhiteSpace(secretKey);

        this._client = new TencentTextTranslateClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            secretId: secretId,
            secretKey: secretKey,
            logger: loggerFactory?.CreateLogger(typeof(TencentTextTranslateService)));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc/>
    public Task<IReadOnlyList<TranslateTextContent>> GetTextTranslateResultAsync(
        string text,
        TranslateExecutionSettings? settings = null,
        CancellationToken? cancellationToken = default)
        => this._client.TranslateTextAsync([text], settings!, cancellationToken);
}
