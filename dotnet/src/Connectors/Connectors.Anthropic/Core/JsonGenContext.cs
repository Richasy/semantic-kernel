// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.Anthropic.Core;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AnthropicContent))]
[JsonSerializable(typeof(AnthropicPart))]
[JsonSerializable(typeof(AnthropicRequest))]
[JsonSerializable(typeof(AnthropicResponse.AnthropicResponseUsage))]
[JsonSerializable(typeof(AnthropicResponse.AnthropicResponseContent))]
[JsonSerializable(typeof(AnthropicResponse))]
[JsonSerializable(typeof(AnthropicStreamResponse))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
