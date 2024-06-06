// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.HunYuan.Core;

internal sealed class HunYuanMessageContent
{
    /// <summary>
    /// The producer of the content. Must be either 'user' or 'assistant' or 'system'.
    /// </summary>
    /// <remarks>Useful to set for multi-turn conversations, otherwise can be left blank or unset.</remarks>
    [JsonPropertyName("Role")]
    [JsonConverter(typeof(AuthorRoleConverter))]
    [JsonRequired]
    public AuthorRole? Role { get; set; }

    [JsonPropertyName("Content")]
    [JsonRequired]
    public string? Content { get; set; }
}
