// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using AlibabaCloud.SDK.Alimt20181012.Models;

namespace Microsoft.SemanticKernel.Translators.Ali.Core;

internal sealed class AliTextTranslateClient
{
    private readonly string _accessKey;
    private readonly string _secret;

    public AliTextTranslateClient(
        string accessKey,
        string secret)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(secret);
        this._accessKey = accessKey;
        this._secret = secret;
    }

    public async Task<IReadOnlyList<TranslateTextContent>> TranslateTextAsync(
               IEnumerable<string> textItems,
               TranslateExecutionSettings settings)
    {
        var results = new List<TranslateTextContent>();
        var config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            AccessKeyId = this._accessKey,
            AccessKeySecret = this._secret,
            Endpoint = "mt.aliyuncs.com",
        };

        var client = new AlibabaCloud.SDK.Alimt20181012.Client(config);
        var aliSettings = (AliTranslateExecutionSettings)settings;
        foreach (var item in textItems)
        {
            var text = item.Replace("\r", "\n").Replace("\n\n", "\n");
            var request = new TranslateGeneralRequest
            {
                FormatType = aliSettings.FormatType.ToString().ToLower(),
                SourceLanguage = aliSettings.From ?? "auto",
                TargetLanguage = aliSettings.To,
                Scene = "general",
                SourceText = text,
            };

            var response = await client.TranslateGeneralAsync(request).ConfigureAwait(false);
            ValidateResponse(response);
            var content = this.ProcessTextTranslateResult(response);
            results.Add(content);
        }

        return results.AsReadOnly();
    }

    private static void ValidateResponse(TranslateGeneralResponse response)
    {
        if (response == null)
        {
            throw new KernelException("Response is null or empty");
        }

        if (response.Body == null || response.Body.Code != 200)
        {
            throw new KernelException("Translation failed");
        }
    }

    private TranslateTextContent ProcessTextTranslateResult(TranslateGeneralResponse response)
    {
        var result = response.Body.Data;
        return new TranslateTextContent
        {
            Source = result.DetectedLanguage,
            Target = default,
            Text = result.Translated,
        };
    }
}
