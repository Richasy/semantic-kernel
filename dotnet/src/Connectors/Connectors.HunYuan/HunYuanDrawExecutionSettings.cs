// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Execution settings for an HunYuan completion request.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class HunYuanDrawExecutionSettings : DrawExecutionSettings
{
    private int _width = 1024;
    private int _height = 1024;

    /// <summary>
    /// Image width.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width
    {
        get => this._width;

        set
        {
            this.ThrowIfFrozen();
            this._width = (int)value;
        }
    }

    /// <summary>
    /// Image height.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height
    {
        get => this._height;

        set
        {
            this.ThrowIfFrozen();
            this._height = (int)value;
        }
    }

    /// <inheritdoc/>
    public override DrawExecutionSettings Clone()
    {
        return new HunYuanDrawExecutionSettings
        {
            Width = this.Width,
            Height = this.Height,
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }

    /// <summary>
    /// Create a new settings object with the values from another settings object.
    /// </summary>
    /// <param name="executionSettings">Template configuration</param>
    /// <returns>An instance of HunYuanDrawExecutionSettings</returns>
    public static HunYuanDrawExecutionSettings FromExecutionSettings(DrawExecutionSettings? executionSettings)
    {
        if (executionSettings is HunYuanDrawExecutionSettings settings)
        {
            return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        var openAIDrawExecutionSettings = JsonSerializer.Deserialize<HunYuanDrawExecutionSettings>(json, JsonOptionsCache.ReadPermissive);
        if (openAIDrawExecutionSettings is not null)
        {
            return openAIDrawExecutionSettings;
        }

        throw new JsonException("Failed to convert execution settings to HunYuanDrawExecutionSettings.");
    }
}
