// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkImageRequest
{
    [JsonPropertyName("header")]
    public SparkRequestHeader? Header { get; set; }

    [JsonPropertyName("parameter")]
    public SparkImageRequestParametersContainer? Parameter { get; set; }

    [JsonPropertyName("payload")]
    public SparkTextRequest.SparkTextRequestPayload? Payload { get; set; }

    internal sealed class SparkImageRequestParametersContainer
    {
        [JsonPropertyName("chat")]
        public SparkImageRequestParameters? Image { get; set; }
    }

    internal sealed class SparkImageRequestParameters
    {
        [JsonPropertyName("domain")]
        public string Domain { get; set; } = "general";

        [JsonPropertyName("width")]
        public int Width { get; set; } = 512;

        [JsonPropertyName("height")]
        public int Height { get; set; } = 512;
    }
}
