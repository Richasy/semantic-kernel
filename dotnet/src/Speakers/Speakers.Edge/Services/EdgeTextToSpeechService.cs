// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Speakers.Edge.Core;
using Microsoft.SemanticKernel.TextToAudio;

namespace Microsoft.SemanticKernel.Speakers.Edge;

/// <summary>
/// Edge 文本转语音服务.
/// </summary>
public sealed class EdgeTextToSpeechService : ITextToAudioService
{
    private readonly Dictionary<string, object?> _attributesInternal = [];
    private readonly EdgeClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeTextToSpeechService"/> class.
    /// </summary>
    public EdgeTextToSpeechService()
    {
        this._client = new EdgeClient();
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AudioContent>> GetAudioContentsAsync(string text, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var content = await this._client.GenerateAudioAsync(text, (EdgeTextToAudioExecutionSettings)executionSettings, cancellationToken).ConfigureAwait(false);
        return [content];
    }
}
