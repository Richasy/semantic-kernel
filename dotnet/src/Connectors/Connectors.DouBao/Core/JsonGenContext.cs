// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.DouBao.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DouBaoMessageContent))]
[JsonSerializable(typeof(DouBaoChatRequest))]
[JsonSerializable(typeof(DouBaoChatResponse))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
