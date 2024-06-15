// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Speakers.Azure.Core;
using Microsoft.SemanticKernel.TextToAudio;

namespace Microsoft.SemanticKernel.Speakers.Azure;

/// <summary>
/// Azure 文本转语音服务.
/// </summary>
public sealed class AzureTextToSpeechService : ITextToAudioService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly AzureTextToSpeechClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureTextToSpeechService"/> class.
    /// </summary>
    public AzureTextToSpeechService(
        string accessKey,
        string region,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(region);

        this._client = new AzureTextToSpeechClient(
            httpClient: HttpClientProvider.GetHttpClient(httpClient),
            accessKey: accessKey,
            region: region,
            logger: loggerFactory?.CreateLogger(typeof(AzureTextToSpeechService)));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AudioContent>> GetAudioContentsAsync(string text, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var content = await this._client.GenerateAudioAsync(text, executionSettings!, cancellationToken).ConfigureAwait(false);
        return [content];
    }
}
