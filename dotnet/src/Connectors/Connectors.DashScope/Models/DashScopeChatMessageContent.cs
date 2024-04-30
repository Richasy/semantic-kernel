// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.DashScope;

/// <summary>
/// DashScope specialized chat message content
/// </summary>
public sealed class DashScopeChatMessageContent : ChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashScopeChatMessageContent"/> class.
    /// </summary>
    /// <param name="calledToolResult">The result of tool called by the kernel.</param>
    public DashScopeChatMessageContent(DashScopeFunctionToolResult calledToolResult)
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
    /// Initializes a new instance of the <see cref="DashScopeChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="calledToolResult">The result of tool called by the kernel.</param>
    /// <param name="metadata">Additional metadata</param>
    internal DashScopeChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        DashScopeFunctionToolResult? calledToolResult = null,
        DashScopeMetadata? metadata = null)
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
    /// Initializes a new instance of the <see cref="DashScopeChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="functionsToolCalls">Tool calls parts returned by model</param>
    /// <param name="metadata">Additional metadata</param>
    internal DashScopeChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        IEnumerable<DashScopePart.FunctionCallPart>? functionsToolCalls,
        DashScopeMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
        this.ToolCalls = functionsToolCalls?.Select(tool => new DashScopeFunctionToolCall(tool)).ToList();
    }

    /// <summary>
    /// A list of the tools returned by the model with arguments.
    /// </summary>
    public IReadOnlyList<DashScopeFunctionToolCall>? ToolCalls { get; }

    /// <summary>
    /// The result of tool called by the kernel.
    /// </summary>
    public DashScopeFunctionToolResult? CalledToolResult { get; }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    public new DashScopeMetadata? Metadata => (DashScopeMetadata?)base.Metadata;
}
