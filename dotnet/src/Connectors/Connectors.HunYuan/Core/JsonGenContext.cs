// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.HunYuan.Core;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(HunYuanChatRequest))]
[JsonSerializable(typeof(HunYuanChatResponse.HunYuanMessageResponse.HunYuanErrorMessage))]
[JsonSerializable(typeof(HunYuanChatResponse.HunYuanMessageResponse.HunYuanResponseChoice))]
[JsonSerializable(typeof(HunYuanChatResponse.HunYuanMessageResponse.HunYuanUsage))]
[JsonSerializable(typeof(HunYuanChatResponse.HunYuanMessageResponse))]
[JsonSerializable(typeof(HunYuanChatResponse))]
[JsonSerializable(typeof(HunYuanDrawCreateRequest))]
[JsonSerializable(typeof(HunYuanDrawCreateResponse))]
[JsonSerializable(typeof(HunYuanDrawQueryRequest))]
[JsonSerializable(typeof(HunYuanDrawQueryResponse))]
[JsonSerializable(typeof(HunYuanMessageContent))]
[JsonSerializable(typeof(HunYuanDrawExecutionSettings))]
[JsonSerializable(typeof(HunYuanPromptExecutionSettings))]
internal partial class JsonGenContext : JsonSerializerContext
{
}
