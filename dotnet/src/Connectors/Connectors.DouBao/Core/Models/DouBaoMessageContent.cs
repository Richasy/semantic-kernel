// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.DouBao.Core;

internal sealed class DouBaoMessageContent
{
    /// <summary>
    /// The producer of the content. Must be either 'user' or 'assistant' or 'system'.
    /// </summary>
    /// <remarks>Useful to set for multi-turn conversations, otherwise can be left blank or unset.</remarks>
    [JsonPropertyName("role")]
    [JsonConverter(typeof(AuthorRoleConverter))]
    [JsonRequired]
    public AuthorRole? Role { get; set; }

    [JsonPropertyName("content")]
    [JsonRequired]
    public string? Content { get; set; }
}
