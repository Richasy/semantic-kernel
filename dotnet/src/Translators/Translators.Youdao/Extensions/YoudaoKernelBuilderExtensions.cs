// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Youdao;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Youdao 内核构建器扩展.
/// </summary>
public static class YoudaoKernelBuilderExtensions
{
    /// <summary>
    /// 添加 Youdao 文本翻译服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddYoudaoTextTranslation(
        this IKernelBuilder builder,
        string appId,
        string secret,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(appId);
        Verify.NotNull(secret);

        builder.Services.AddKeyedSingleton<ITextTranslateService>(
            serviceId,
            (serviceProvider, _) =>
            {
                return new YoudaoTextTranslateService(
                        appId,
                        secret,
                        httpClient,
                        serviceProvider.GetService<ILoggerFactory>());
            });
        return builder;
    }
}
