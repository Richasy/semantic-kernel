// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.HunYuan;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Extensions for adding Tencent AI generation services to the application.
/// </summary>
public static class HunYuanKernelBuilderExtensions
{
    /// <summary>
    /// Add Tencent AI HunYuan Chat Completion and Text Generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The kernel builder.</param>
    /// <param name="modelId">The model for text generation.</param>
    /// <param name="secretId">The secret ID for authentication HunYuan API.</param>
    /// <param name="secretKey">The secret key for authentication HunYuan API.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <param name="httpClient">The optional custom HttpClient.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddHunYuanChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string secretId,
        string secretKey,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(modelId);
        Verify.NotNull(secretId);
        Verify.NotNull(secretKey);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
            new HunYuanChatCompletionService(
                modelId: modelId,
                secretId: secretId,
                secretKey: secretKey,
                httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider),
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }
}
