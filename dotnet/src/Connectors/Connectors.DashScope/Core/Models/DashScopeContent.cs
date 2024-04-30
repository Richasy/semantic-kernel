// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.DashScope.Core;
internal class DashScopeContent
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

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<ToolCallResponse>? ToolCalls { get; set; }

    internal sealed class ToolCallResponse
    {
        [JsonPropertyName("function")]
        public FunctionCallPart? Function { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    /// <summary>
    /// A predicted FunctionCall returned from the model that contains a
    /// string representing the FunctionDeclaration.name with the arguments and their values.
    /// </summary>
    internal sealed class FunctionCallPart
    {
        /// <summary>
        /// Required. The name of the function to call. Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 63.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonRequired]
        public string FunctionName { get; set; } = null!;

        /// <summary>
        /// Optional. The function parameters and values in JSON object format.
        /// </summary>
        [JsonPropertyName("arguments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonNode? Arguments { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"FunctionName={this.FunctionName}, Arguments={this.Arguments}";
        }
    }
}
