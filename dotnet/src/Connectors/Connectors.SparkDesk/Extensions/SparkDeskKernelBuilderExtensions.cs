// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.SparkDesk;

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
    /// <param name="apiVersion">The version of the Spark API.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddSparkDeskChatCompletion(
        this IKernelBuilder builder,
        string apiKey,
        string secret,
        string appId,
        SparkDeskAIVersion apiVersion = SparkDeskAIVersion.V3_5,
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
                apiVersion: apiVersion,
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }
}
