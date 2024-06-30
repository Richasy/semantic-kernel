// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.DouBao;

/// <summary>
/// DouBao specialized chat message content
/// </summary>
public sealed class DouBaoChatMessageContent : ChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DouBaoChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="metadata">Additional metadata</param>
    internal DouBaoChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        DouBaoMetadata? metadata = null)
        : base(
            role: role,
            content: content,
            modelId: modelId,
            innerContent: content,
            encoding: System.Text.Encoding.UTF8,
            metadata: metadata)
    {
    }

    /// <summary>
    /// The metadata associated with the content.
    /// </summary>
    public new DouBaoMetadata? Metadata => (DouBaoMetadata?)base.Metadata;
}
