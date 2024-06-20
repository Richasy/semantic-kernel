// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Youdao.Core;

namespace Microsoft.SemanticKernel.Translators.Youdao;

/// <summary>
/// Youdao 文本翻译服务.
/// </summary>
public sealed class YoudaoTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly YoudaoTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="YoudaoTextTranslateService"/> class.
    /// </summary>
    public YoudaoTextTranslateService(
        string appId,
        string secret,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(appId);
        Verify.NotNullOrWhiteSpace(secret);

        this._client = new YoudaoTextTranslateClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            appId: appId,
            secret: secret,
            logger: loggerFactory?.CreateLogger(typeof(YoudaoTextTranslateService)));
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
