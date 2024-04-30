// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Spark desk specialized chat message content.
/// </summary>
public sealed class SparkChatMessageContent : ChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkChatMessageContent"/> class.
    /// </summary>
    /// <param name="calledToolResult">The result of tool called by the kernel.</param>
    public SparkChatMessageContent(SparkFunctionToolResult calledToolResult)
        : base(
            role: AuthorRole.Tool,
            content: null,
            modelId: null,
            innerContent: null,
            encoding: Encoding.UTF8,
            metadata: null)
    {
        Verify.NotNull(calledToolResult);

        this.CalledToolResult = calledToolResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="calledToolResult">The result of tool called by the kernel.</param>
    /// <param name="metadata">Additional metadata</param>
    internal SparkChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        SparkFunctionToolResult? calledToolResult = null,
        SparkMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
        this.CalledToolResult = calledToolResult;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SparkChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="functionsToolCalls">Tool calls parts returned by model</param>
    /// <param name="metadata">Additional metadata</param>
    internal SparkChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        IEnumerable<SparkFunctionCall>? functionsToolCalls,
        SparkMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
        this.ToolCalls = functionsToolCalls?.Select(tool => new SparkFunctionToolCall(tool)).ToList();
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
