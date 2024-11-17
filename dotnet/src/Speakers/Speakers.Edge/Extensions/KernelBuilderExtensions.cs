// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Speakers.Edge;
using Microsoft.SemanticKernel.TextToAudio;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Kernel builder.
/// </summary>
public static class KernelBuilderExtensions
{
    /// <summary>
    /// 添加 Edge 文本转语音服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddEdgeTextToSpeech(
        this IKernelBuilder builder,
        string? serviceId = null)
    {
        Verify.NotNull(builder);

        builder.Services.AddKeyedSingleton<ITextToAudioService>(
            serviceId,
            (serviceProvider, _) =>
            {
                return new EdgeTextToSpeechService();
            });
        return builder;
    }
}
