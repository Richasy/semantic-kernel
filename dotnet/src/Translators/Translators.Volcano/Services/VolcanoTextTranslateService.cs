// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Volcano.Core;

namespace Microsoft.SemanticKernel.Translators.Volcano;

/// <summary>
/// Volcano 文本翻译服务.
/// </summary>
public sealed class VolcanoTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly VolcanoTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="VolcanoTextTranslateService"/> class.
    /// </summary>
    public VolcanoTextTranslateService(
        string secretId,
        string secretKey,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(secretId);
        Verify.NotNullOrWhiteSpace(secretKey);

        this._client = new VolcanoTextTranslateClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            secretId: secretId,
            secretKey: secretKey,
            logger: loggerFactory?.CreateLogger(typeof(VolcanoTextTranslateService)));
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
