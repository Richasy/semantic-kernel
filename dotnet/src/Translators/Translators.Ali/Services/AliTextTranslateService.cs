// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.SemanticKernel.Translate;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel.Translators.Ali.Core;

namespace Microsoft.SemanticKernel.Translators.Ali;

/// <summary>
/// Ali 文本翻译服务.
/// </summary>
public sealed class AliTextTranslateService : ITextTranslateService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly AliTextTranslateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AliTextTranslateService"/> class.
    /// </summary>
    public AliTextTranslateService(
        string accessKey,
        string secret)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(secret);

        this._client = new AliTextTranslateClient(
            accessKey: accessKey,
            secret: secret);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc/>
    public Task<IReadOnlyList<TranslateTextContent>> GetTextTranslateResultAsync(
        string text,
        TranslateExecutionSettings? settings = null,
        CancellationToken? cancellationToken = default)
        => this._client.TranslateTextAsync([text], settings!);
}
