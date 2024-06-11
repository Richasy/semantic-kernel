// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.QianFan;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Extensions for adding BaiduAI generation services to the application.
/// </summary>
public static class QianFanKernelBuilderExtensions
{
    /// <summary>
    /// Add Baidu AI QianFan Chat Completion and Text Generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The kernel builder.</param>
    /// <param name="modelId">The model for text generation.</param>
    /// <param name="apiKey">The API key for authentication Gemini API.</param>
    /// <param name="apiSecret">The secret of the Baidu API.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <param name="httpClient">The optional custom HttpClient.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddQianFanChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        string apiSecret,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(modelId);
        Verify.NotNull(apiKey);
        Verify.NotNull(apiSecret);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
            new QianFanChatCompletionService(
                modelId: modelId,
                apiKey: apiKey,
                apiSecret: apiSecret,
                httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider),
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }

    /// <summary>
    /// Add Baidu AI QianFan Image generation services to the kernel builder.
    /// </summary>
    public static IKernelBuilder AddQianFanImageGeneration(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        string apiSecret,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(modelId);
        Verify.NotNull(apiKey);
        Verify.NotNull(apiSecret);

        builder.Services.AddKeyedSingleton<ITextToImageService>(serviceId, (serviceProvider, _) =>
            new QianFanImageGenerationService(
                modelId: modelId,
                apiKey: apiKey,
                apiSecret: apiSecret,
                httpClient: HttpClientProvider.GetHttpClient(httpClient, serviceProvider),
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }
}
