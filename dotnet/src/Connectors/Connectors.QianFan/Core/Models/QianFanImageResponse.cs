// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

internal sealed class QianFanImageResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("data")]
    public IList<QianFanImageItem>? Data { get; set; }

    internal sealed class QianFanImageItem
    {
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("b64_image")]
        public string? Base64 { get; set; }

        [JsonPropertyName("index")]
        public int? Index { get; set; }
    }
}
