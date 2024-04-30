// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal sealed class QianFanMessageContent
{
    /// <summary>
    /// The producer of the content. Must be either 'user' or 'model' or 'function'.
    /// </summary>
    /// <remarks>Useful to set for multi-turn conversations, otherwise can be left blank or unset.</remarks>
    [JsonPropertyName("role")]
    [JsonConverter(typeof(AuthorRoleConverter))]
    [JsonRequired]
    public AuthorRole? Role { get; set; }

    [JsonPropertyName("content")]
    [JsonRequired]
    public string? Content { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }
}
