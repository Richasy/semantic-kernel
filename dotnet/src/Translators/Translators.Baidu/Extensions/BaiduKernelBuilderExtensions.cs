// Copyright (c) Richasy. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Baidu;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Baidu 内核构建器扩展.
/// </summary>
public static class BaiduKernelBuilderExtensions
{
    /// <summary>
    /// 添加 Baidu 文本翻译服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddBaiduTextTranslation(
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
                return new BaiduTextTranslateService(
                        appId,
                        secret,
                        httpClient,
                        serviceProvider.GetService<ILoggerFactory>());
            });
        return builder;
    }
}
