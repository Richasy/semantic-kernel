// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Translators.Azure.Core;

internal sealed class AzureTextTranslateClient : ClientBase
{
    private readonly string _accessKey;
    private readonly string _region;

    public AzureTextTranslateClient(
        HttpClient httpClient,
        string accessKey,
        string region,
        ILogger? logger = null)
        : base(httpClient, logger)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(region);

        this._accessKey = accessKey;
        this._region = region;
    }

    public async Task<IReadOnlyList<TranslateTextContent>> TranslateTextAsync(
        IEnumerable<string> textItems,
        TranslateExecutionSettings settings,
        CancellationToken? cancellationToken = default)
    {
        var requestObject = this.CreateTextTranslateRequest(textItems);
        using var request = this.CreateHttpRequest(requestObject, (AzureTranslateExecutionSettings)settings, this._accessKey, this._region);
        var body = await this.SendRequestAndGetStringBodyAsync(request, cancellationToken ?? CancellationToken.None)
            .ConfigureAwait(false);
        var response = DeserializeResponse<List<TextTranslateResponse>>(body);
        ValidateResponse(response);
        return this.ProcessTextTranslateResult(response).AsReadOnly();
    }

    private static void ValidateResponse(List<TextTranslateResponse> response)
    {
        if (response == null || response.Count == 0)
        {
            throw new KernelException("Response is null or empty");
        }

        foreach (var item in response)
        {
            if (item.Translations == null || item.Translations.Count == 0)
            {
                throw new KernelException("Translation is null or empty");
            }
        }
    }

    private List<TranslateTextContent> ProcessTextTranslateResult(List<TextTranslateResponse> response)
    {
        var result = new List<TranslateTextContent>();
        foreach (var item in response)
        {
            var firstTranslate = item.Translations.FirstOrDefault();
            var t = new TranslateTextContent
            {
                Text = firstTranslate?.Text,
                Source = item.DetectedLanguage?.Language,
                Target = firstTranslate?.To,
            };
            result.Add(t);
        }

        return result;
    }

    private TextTranslateRequest CreateTextTranslateRequest(IEnumerable<string> textItems)
    {
        var request = new TextTranslateRequest();
        foreach (var text in textItems)
        {
            request.AddText(text);
        }

        return request;
    }
}
