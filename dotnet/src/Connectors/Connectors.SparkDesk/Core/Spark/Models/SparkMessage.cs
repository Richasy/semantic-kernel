// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

/// <summary>
/// Represents a function parameter that can be passed to an SparkDesk function tool call.
/// </summary>
internal sealed class SparkMessage
{
    /// <summary>
    /// Text messages.
    /// </summary>
    [JsonPropertyName("text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonRequired]
    public IList<SparkTextMessage>? Text { get; set; }

    internal class SparkTextMessage
    {
        /// <summary>
        /// Message content.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// The producer of the content. Must be either 'user' or 'assistant' or 'system' pr 'tool'.
        /// </summary>
        /// <remarks>Useful to set for multi-turn conversations, otherwise can be left blank or unset.</remarks>
        [JsonPropertyName("role")]
        [JsonConverter(typeof(AuthorRoleConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonRequired]
        public AuthorRole? Role { get; set; }
    }
}
