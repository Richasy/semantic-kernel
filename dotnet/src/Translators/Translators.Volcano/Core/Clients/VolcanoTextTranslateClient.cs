// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Translators.Volcano.Core;

internal sealed class VolcanoTextTranslateClient : ClientBase
{
    private readonly string _secretId;
    private readonly string _secretKey;

    public VolcanoTextTranslateClient(
        HttpClient httpClient,
        string secretId,
        string secretKey,
        ILogger? logger = null)
        : base(httpClient, logger)
    {
        Verify.NotNullOrWhiteSpace(secretKey);
        Verify.NotNullOrWhiteSpace(secretId);

        this._secretId = secretId;
        this._secretKey = secretKey;
    }

    public async Task<IReadOnlyList<TranslateTextContent>> TranslateTextAsync(
               IEnumerable<string> textItems,
               TranslateExecutionSettings settings,
               CancellationToken? cancellationToken = default)
    {
        var textList = new List<string>();
        foreach (var item in textItems)
        {
            var text = item.Replace("\r", "\n").Replace("\n\n", "\n");
            textList.Add(text);
        }

        var volcanoSettings = (VolcanoTranslateExecutionSettings)settings;
        var req = new TextTranslateRequest
        {
            SourceLanguage = volcanoSettings.From,
            TargetLanguage = volcanoSettings.To,
            TextList = textList,
        };
        using var request = this.CreateHttpRequest(req, this._secretKey, this._secretId);
        var body = await this.SendRequestAndGetStringBodyAsync(request, cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);
        var response = DeserializeResponse<TextTranslateResponse>(body);
        ValidateResponse(response);
        return this.ProcessTextTranslateResult(response).ToList().AsReadOnly();
    }

    private static void ValidateResponse(TextTranslateResponse response)
    {
        if (response == null || response.TranslationList == null)
        {
            throw new KernelException("Response is null or empty");
        }
    }

    private IEnumerable<TranslateTextContent> ProcessTextTranslateResult(TextTranslateResponse response)
    {
        foreach (var item in response.TranslationList!)
        {
            yield return new TranslateTextContent
            {
                Source = item.DetectedSourceLanguage,
                Text = item.Translation,
            };
        }
    }
}
