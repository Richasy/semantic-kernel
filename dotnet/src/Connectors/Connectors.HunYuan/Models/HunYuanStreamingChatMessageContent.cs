// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// HunYuan specialized streaming chat message content
/// </summary>
public sealed class HunYuanStreamingChatMessageContent : StreamingChatMessageContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HunYuanStreamingChatMessageContent"/> class.
    /// </summary>
    /// <param name="role">Role of the author of the message</param>
    /// <param name="content">Content of the message</param>
    /// <param name="modelId">The model ID used to generate the content</param>
    /// <param name="metadata">Additional metadata</param>
    internal HunYuanStreamingChatMessageContent(
        AuthorRole role,
        string? content,
        string modelId,
        HunYuanMetadata? metadata = null)
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
    public new HunYuanMetadata? Metadata => (HunYuanMetadata?)base.Metadata;
}
