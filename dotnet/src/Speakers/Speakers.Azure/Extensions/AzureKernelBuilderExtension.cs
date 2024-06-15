// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Speakers.Azure;
using Microsoft.SemanticKernel.TextToAudio;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Azure 内核构建器扩展.
/// </summary>
public static class AzureKernelBuilderExtensions
{
    /// <summary>
    /// 添加 Azure 文本转语音服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddAzureTextToSpeech(
        this IKernelBuilder builder,
        string accessKey,
        string region,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(accessKey);
        Verify.NotNull(region);

        builder.Services.AddKeyedSingleton<ITextToAudioService>(
            serviceId,
            (serviceProvider, _) =>
            {
                return new AzureTextToSpeechService(
                        accessKey,
                        region,
                        httpClient,
                        serviceProvider.GetService<ILoggerFactory>());
            });
        return builder;
    }
}
