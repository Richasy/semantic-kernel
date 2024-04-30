// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.DashScope.Core;

internal sealed class DashScopeResponseChoice
{
    [JsonPropertyName("message")]
    public DashScopeContent? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public DashScopeFinishReason FinishReason { get; set; }
}
