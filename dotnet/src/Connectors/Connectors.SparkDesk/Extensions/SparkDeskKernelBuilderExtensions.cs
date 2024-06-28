// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.SparkDesk;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Extensions for adding SparkDesk generation services to the application.
/// </summary>
public static class SparkDeskKernelBuilderExtensions
{
    /// <summary>
    /// Add Spark Chat Completion and Text Generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The kernel builder.</param>
    /// <param name="apiKey">The API key for authentication Spark API.</param>
    /// <param name="secret">Secret.</param>
    /// <param name="appId">App id.</param>
    /// <param name="modelId">The version of the Spark API.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddSparkDeskChatCompletion(
        this IKernelBuilder builder,
        string apiKey,
        string secret,
        string appId,
        string modelId,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(secret);
        Verify.NotNull(apiKey);
        Verify.NotNull(appId);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
            new SparkDeskChatCompletionService(
                apiKey: apiKey,
                secret: secret,
                appId: appId,
                modelId: modelId,
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }

    /// <summary>
    /// Add Spark Image generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The kernel builder.</param>
    /// <param name="apiKey">The API key for authentication Spark API.</param>
    /// <param name="secret">Secret.</param>
    /// <param name="appId">App id.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddSparkDeskImageGeneration(
        this IKernelBuilder builder,
        string apiKey,
        string secret,
        string appId,
        string version,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(secret);
        Verify.NotNull(apiKey);
        Verify.NotNull(appId);

        builder.Services.AddKeyedSingleton<ITextToImageService>(serviceId, (serviceProvider, _) =>
            new SparkDeskImageGenerationService(
                apiKey: apiKey,
                secret: secret,
                appId: appId,
                version: version,
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }
}
