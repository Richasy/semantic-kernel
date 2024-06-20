// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Translators.Tencent.Core;

internal sealed class TencentTextTranslateClient : ClientBase
{
    private readonly string _secretId;
    private readonly string _secretKey;

    public TencentTextTranslateClient(
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
        var results = new List<TranslateTextContent>();
        foreach (var item in textItems)
        {
            var text = item.Replace("\r", "\n").Replace("\n\n", "\n");
            var tencentSettings = (TencentTranslateExecutionSettings)settings;
            var req = new TextTranslateRequest
            {
                SourceText = text,
                Source = tencentSettings.From ?? "auto",
                Target = tencentSettings.To,
                ProjectId = 0,
                UntranslatedText = tencentSettings.UntranslatedText,
            };
            using var request = this.CreateHttpRequest(req, this._secretKey, this._secretId);
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
        if (response == null || response.Response == null)
        {
            throw new KernelException("Response is null or empty");
        }
    }

    private TranslateTextContent ProcessTextTranslateResult(TextTranslateResponse response)
    {
        var result = response.Response;
        return new TranslateTextContent
        {
            Source = result!.Source,
            Target = result!.Target,
            Text = result!.TargetText,
        };
    }
}
