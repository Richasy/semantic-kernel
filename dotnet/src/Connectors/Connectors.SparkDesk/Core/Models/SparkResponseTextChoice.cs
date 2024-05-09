// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkResponseTextChoice : SparkMessage.SparkTextMessage
{
    /// <summary>
    /// Index of the choice in the list of choices.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("function_call")]
    public SparkFunctionCall? FunctionCall { get; set; }
}
