// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

/// <summary>
/// Anthropic specialized chat message content
/// </summary>
public sealed class AnthropicChatMessageContent : ChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnthropicChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="metadata">Additional metadata</param>
    internal AnthropicChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        AnthropicMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            encoding: Encoding.UTF8,
            metadata: metadata)
    {
    }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    public new AnthropicMetadata? Metadata => (AnthropicMetadata?)base.Metadata;
}
