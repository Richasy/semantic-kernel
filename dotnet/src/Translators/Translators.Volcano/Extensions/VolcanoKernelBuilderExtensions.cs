// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Volcano;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Volcano 内核构建器扩展.
/// </summary>
public static class VolcanoKernelBuilderExtensions
{
    /// <summary>
    /// 添加 Volcano 文本翻译服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddVolcanoTextTranslation(
        this IKernelBuilder builder,
        string secretId,
        string secretKey,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(secretId);
        Verify.NotNull(secretKey);

        builder.Services.AddKeyedSingleton<ITextTranslateService>(
            serviceId,
            (serviceProvider, _) =>
            {
                return new VolcanoTextTranslateService(
                        secretId,
                        secretKey,
                        httpClient,
                        serviceProvider.GetService<ILoggerFactory>());
            });
        return builder;
    }
}
