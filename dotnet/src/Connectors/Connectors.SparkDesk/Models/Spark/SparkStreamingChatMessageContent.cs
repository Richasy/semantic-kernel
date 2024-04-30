// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Spark specialized streaming chat message content
/// </summary>
public sealed class SparkStreamingChatMessageContent : StreamingChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkStreamingChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="calledToolResult">The result of tool called by the kernel.</param>
    /// <param name="metadata">Additional metadata</param>
    internal SparkStreamingChatMessageContent(
        AuthorRole? role,
        string? content,
        string modelId,
        SparkFunctionToolResult? calledToolResult = null,
        SparkMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            innerContent: content,
            choiceIndex: 0,
            modelId: modelId,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
        this.CalledToolResult = calledToolResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkStreamingChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="toolCalls">Tool calls returned by model</param>
    /// <param name="metadata">Additional metadata</param>
    internal SparkStreamingChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        IReadOnlyList<SparkFunctionToolCall>? toolCalls,
        SparkMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            choiceIndex: 0,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
        this.ToolCalls = toolCalls;
    }

    /// <summary>
    /// A list of the tools returned by the model with arguments.
    /// </summary>
    public IReadOnlyList<SparkFunctionToolCall>? ToolCalls { get; }

    /// <summary>
    /// The result of tool called by the kernel.
    /// </summary>
    public SparkFunctionToolResult? CalledToolResult { get; }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    public new SparkMetadata? Metadata => (SparkMetadata?)base.Metadata;
}
