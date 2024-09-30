// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.Google.Core;

namespace Microsoft.SemanticKernel.Connectors.Google;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GeminiContent))]
[JsonSerializable(typeof(GeminiPart.FileDataPart))]
[JsonSerializable(typeof(GeminiPart.FunctionCallPart))]
[JsonSerializable(typeof(GeminiPart.FunctionResponsePart))]
[JsonSerializable(typeof(GeminiPart.InlineDataPart))]
[JsonSerializable(typeof(GeminiPart))]
[JsonSerializable(typeof(GeminiRequest))]
[JsonSerializable(typeof(GeminiSafetyRating))]
[JsonSerializable(typeof(GeminiResponseCandidate))]
[JsonSerializable(typeof(GeminiFunctionToolResult))]
[JsonSerializable(typeof(GeminiResponse))]
[JsonSerializable(typeof(GeminiTool))]
[JsonSerializable(typeof(GoogleAIEmbeddingResponse))]
[JsonSerializable(typeof(GoogleAIEmbeddingRequest))]
[JsonSerializable(typeof(VertexAIEmbeddingResponse))]
[JsonSerializable(typeof(VertexAIEmbeddingRequest))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
