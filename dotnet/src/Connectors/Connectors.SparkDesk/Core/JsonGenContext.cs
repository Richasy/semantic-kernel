// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SparkTextRequest))]
[JsonSerializable(typeof(SparkTextResponse))]
[JsonSerializable(typeof(SparkTextResponse.SparkResponseText))]
[JsonSerializable(typeof(SparkTextResponse.SparkResponseTextUsage))]
[JsonSerializable(typeof(SparkTextResponse.SparkTextResponseHeader))]
[JsonSerializable(typeof(SparkTextResponse.SparkTextResponsePayload))]
[JsonSerializable(typeof(SparkFunctionCall))]
[JsonSerializable(typeof(SparkFunctionParameter))]
[JsonSerializable(typeof(SparkFunctionReturnParameter))]
[JsonSerializable(typeof(SparkFunctionToolCall))]
[JsonSerializable(typeof(SparkFunctionToolResult))]
[JsonSerializable(typeof(SparkDeskFunction))]
[JsonSerializable(typeof(SparkImageRequest))]
[JsonSerializable(typeof(SparkMessage))]
[JsonSerializable(typeof(SparkMetadata))]
[JsonSerializable(typeof(SparkResponseTextChoice))]
[JsonSerializable(typeof(SparkUsage))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
