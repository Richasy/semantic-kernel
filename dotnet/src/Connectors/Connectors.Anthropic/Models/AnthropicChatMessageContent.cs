// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

/// <summary>
/// Anthropic specialized streaming chat message content
/// </summary>
public sealed class AnthropicStreamingChatMessageContent : StreamingChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnthropicStreamingChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="metadata">Additional metadata</param>
    internal AnthropicStreamingChatMessageContent(
        AuthorRole? role,
        string? content,
        string modelId,
        AnthropicMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            innerContent: content,
            modelId: modelId,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
    }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    public new AnthropicMetadata? Metadata => (AnthropicMetadata?)base.Metadata;
}
