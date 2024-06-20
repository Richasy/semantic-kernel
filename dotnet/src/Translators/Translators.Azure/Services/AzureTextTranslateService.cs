// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Azure.Core;

namespace Microsoft.SemanticKernel.Translators.Azure;

/// <summary>
/// Azure 文本翻译服务.
/// </summary>
public sealed class AzureTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly AzureTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTextTranslateService"/> class.
    /// </summary>
    public AzureTextTranslateService(
        string accessKey,
        string region,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(region);

        this._client = new AzureTextTranslateClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            accessKey: accessKey,
            region: region,
            logger: loggerFactory?.CreateLogger(typeof(AzureTextTranslateService)));
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
