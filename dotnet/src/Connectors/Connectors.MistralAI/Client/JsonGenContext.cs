// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.MistralAI.Client;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ChatCompletionRequest))]
[JsonSerializable(typeof(MistralChatMessage))]
[JsonSerializable(typeof(MistralChatChoice))]
[JsonSerializable(typeof(MistralChatCompletionChoice))]
[JsonSerializable(typeof(MistralChatCompletionChunk))]
[JsonSerializable(typeof(MistralParameters))]
[JsonSerializable(typeof(MistralTool))]
[JsonSerializable(typeof(MistralToolCall))]
[JsonSerializable(typeof(MistralUsage))]
[JsonSerializable(typeof(ChatCompletionResponse))]
[JsonSerializable(typeof(TextEmbeddingRequest))]
[JsonSerializable(typeof(TextEmbeddingResponse))]
[JsonSerializable(typeof(KernelArguments))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
