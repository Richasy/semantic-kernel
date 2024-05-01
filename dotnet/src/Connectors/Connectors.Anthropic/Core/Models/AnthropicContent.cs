// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.Anthropic.Core;

internal sealed class AnthropicContent
{
    [JsonPropertyName("content")]
    public IList<AnthropicPart>? Content { get; set; }

    [JsonPropertyName("role")]
    [JsonConverter(typeof(AuthorRoleConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthorRole? Role { get; set; }
}
