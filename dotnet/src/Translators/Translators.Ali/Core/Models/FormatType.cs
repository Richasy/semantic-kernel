// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Translators.Ali.Core;

/// <summary>
/// 格式类型.
/// </summary>
[JsonConverter(typeof(FormatTypeConverter))]
public enum FormatType
{
    /// <summary>
    /// Html.
    /// </summary>
    Html,

    /// <summary>
    /// Text.
    /// </summary>
    Text,
}

internal sealed class FormatTypeConverter : JsonConverter<FormatType>
{
    public override FormatType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "html" => FormatType.Html,
            "text" => FormatType.Text,
            _ => FormatType.Text,
        };
    }

    public override void Write(Utf8JsonWriter writer, FormatType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}
