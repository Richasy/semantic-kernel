// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.QianFan.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(QianFanAuthToken))]
[JsonSerializable(typeof(QianFanChatRequest))]
[JsonSerializable(typeof(QianFanChatResponse))]
[JsonSerializable(typeof(QianFanChatResponse.QianFanUsage))]
[JsonSerializable(typeof(QianFanImageRequest))]
[JsonSerializable(typeof(QianFanImageResponse))]
[JsonSerializable(typeof(QianFanImageResponse.QianFanImageItem))]
[JsonSerializable(typeof(QianFanMessageContent))]
[JsonSerializable(typeof(QianFanTokenContext))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
