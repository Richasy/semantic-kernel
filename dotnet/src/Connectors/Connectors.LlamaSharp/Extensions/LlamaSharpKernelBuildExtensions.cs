// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.LlamaSharp;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Extensions for adding LlamaSharp generation services to the application.
/// </summary>
public static class LlamaSharpKernelBuildExtensions
{
    /// <summary>
    /// Add Google AI Gemini Chat Completion and Text Generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The kernel builder.</param>
    /// <param name="modelPath">The model path for text generation.</param>
    /// <param name="serviceId">The optional service ID.</param>
    /// <returns>The updated kernel builder.</returns>
    public static IKernelBuilder AddLlamaSharpChatCompletion(
        this IKernelBuilder builder,
        string modelPath,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNull(modelPath);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
            new LlamaSharpChatCompletionService(
                modelPath: modelPath,
                loggerFactory: serviceProvider.GetService<ILoggerFactory>()));
        return builder;
    }
}
