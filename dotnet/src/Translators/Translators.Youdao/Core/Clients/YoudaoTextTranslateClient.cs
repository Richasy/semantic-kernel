﻿// Copyright (c) Richasy. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Translators.Youdao.Core;

internal sealed class YoudaoTextTranslateClient : ClientBase
{
    public YoudaoTextTranslateClient(
        HttpClient httpClient,
        string appId,
        string secret,
        ILogger? logger = null)
        : base(httpClient, logger, appId, secret)
    {
        Verify.NotNullOrWhiteSpace(appId);
        Verify.NotNullOrWhiteSpace(secret);
    }

    public async Task<IReadOnlyList<TranslateTextContent>> TranslateTextAsync(
               IEnumerable<string> textItems,
               TranslateExecutionSettings settings,
               CancellationToken? cancellationToken = default)
    {
        var results = new List<TranslateTextContent>();
        foreach (var item in textItems)
        {
            var text = item.Replace("\r", "\n").Replace("\n\n", "\n");
            using var request = this.CreateHttpRequest(item, (YoudaoTranslateExecutionSettings)settings);
            var body = await this.SendRequestAndGetStringBodyAsync(request, cancellationToken ?? CancellationToken.None)
                .ConfigureAwait(false);
            var response = DeserializeResponse<TextTranslateResponse>(body);
            ValidateResponse(response);
            results.Add(this.ProcessTextTranslateResult(response));
        }

        return results.AsReadOnly();
    }

    private static void ValidateResponse(TextTranslateResponse response)
    {
        if (response == null)
        {
            throw new KernelException("Response is null or empty");
        }

        if (response.Translation == null || response.Translation.Length == 0)
        {
            throw new KernelException("Translation is null or empty");
        }
    }

    private TranslateTextContent ProcessTextTranslateResult(TextTranslateResponse response)
    {
        var result = response.Translation;
        var langSplit = response.Language?.Split('2');
        return new TranslateTextContent
        {
            Source = langSplit?.FirstOrDefault(),
            Target = langSplit?.LastOrDefault(),
            Text = result.FirstOrDefault(),
        };
    }
}
