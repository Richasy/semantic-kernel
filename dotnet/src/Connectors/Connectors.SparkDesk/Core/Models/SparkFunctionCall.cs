// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

/// <summary>
/// A predicted FunctionCall returned from the model that contains a
/// string representing the FunctionDeclaration.name with the arguments and their values.
/// </summary>
internal sealed class SparkFunctionCall
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
