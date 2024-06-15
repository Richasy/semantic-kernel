// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Speakers.Azure.Core;

internal sealed class AzureTextToSpeechClient : ClientBase
{
    private readonly string _accessKey;
    private readonly string _region;

    public AzureTextToSpeechClient(
        HttpClient httpClient,
        string accessKey,
        string region,
        ILogger? logger = null)
        : base(region, httpClient, logger)
    {
        Verify.NotNullOrWhiteSpace(accessKey);
        Verify.NotNullOrWhiteSpace(region);

        this._accessKey = accessKey;
        this._region = region;
    }

    public async Task<AudioContent> GenerateAudioAsync(string text, PromptExecutionSettings settings, CancellationToken? cancellationToken = default)
    {
        using var request = this.CreateHttpRequest(text, (AzureTextToAudioExecutionSettings)settings, this._accessKey);
        var data = await this.SendRequestAndGetBytesAsync(request, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        return data == null ? throw new KernelException("Failed to generate audio content.") : new AudioContent(data);
    }
}
