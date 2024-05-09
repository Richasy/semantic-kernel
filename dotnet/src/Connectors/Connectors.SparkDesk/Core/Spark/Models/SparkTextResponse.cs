// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

/// <summary>
/// Response from the model supporting multiple candidates.
/// </summary>
internal sealed class SparkTextResponse
{
    [JsonPropertyName("header")]
    public SparkTextResponseHeader? Header { get; set; }

    [JsonPropertyName("payload")]
    public SparkTextResponsePayload? Payload { get; set; }


    internal sealed class SparkTextResponseHeader
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("sid")]
        public string? Sid { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    internal sealed class SparkTextResponsePayload
    {
        [JsonPropertyName("choices")]
        [JsonRequired]
        public SparkResponseText? Choices { get; set; }

        [JsonPropertyName("usage")]
        public SparkResponseTextUsage? Usage { get; set; }
    }

    internal sealed class SparkResponseText
    {
        /// <summary>
        /// Text response status, values are [0, 1, 2]; 0 represents the first text result; 1 represents the intermediate text result; 2 represents the last text result.
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// The sequence number of the returned data, values are [0, 9999999].
        /// </summary>
        [JsonPropertyName("seq")]
        public int Seq { get; set; }

        /// <summary>
        /// An array of text objects.
        /// </summary>
        [JsonPropertyName("text")]
        [JsonRequired]
        public IList<SparkResponseTextChoice>? Text { get; set; }
    }

    internal sealed class SparkResponseTextUsage
    {
        [JsonPropertyName("text")]
        public SparkUsage? Text { get; set; }
    }
}
