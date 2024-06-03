// Copyright (c) Richasy. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Translators.Baidu.Core;

namespace Microsoft.SemanticKernel.Translators.Baidu;

/// <summary>
/// Baidu 文本翻译服务.
/// </summary>
public sealed class BaiduTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly BaiduTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaiduTextTranslateService"/> class.
    /// </summary>
    public BaiduTextTranslateService(
        string appId,
        string secret,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(appId);
        Verify.NotNullOrWhiteSpace(secret);

        this._client = new BaiduTextTranslateClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            appId: appId,
            secret: secret,
            logger: loggerFactory?.CreateLogger(typeof(BaiduTextTranslateService)));
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
