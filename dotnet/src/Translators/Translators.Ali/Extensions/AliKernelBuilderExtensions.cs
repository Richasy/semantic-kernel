// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Translate;
using Microsoft.SemanticKernel.Translators.Ali;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Ali 内核构建器扩展.
/// </summary>
public static class AliKernelBuilderExtensions
{
    /// <summary>
    /// 添加 Ali 文本翻译服务.
    /// </summary>
    /// <returns>更新后的 <see cref="IKernelBuilder"/>.</returns>
    public static IKernelBuilder AddAliTextTranslation(
        this IKernelBuilder builder,
        string accessKey,
        string secret,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(accessKey);
        Verify.NotNull(secret);

        builder.Services.AddKeyedSingleton<ITextTranslateService>(
            serviceId,
            (serviceProvider, _) =>
            {
                return new AliTextTranslateService(
                        accessKey,
                        secret);
            });
        return builder;
    }
}
