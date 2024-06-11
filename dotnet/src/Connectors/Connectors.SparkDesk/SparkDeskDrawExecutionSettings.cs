// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Execution settings for an SparkDesk completion request.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class SparkDeskDrawExecutionSettings : DrawExecutionSettings
{
    private int _width = 512;
    private int _height = 512;

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
        return new SparkDeskDrawExecutionSettings
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
    /// <returns>An instance of SparkDeskDrawExecutionSettings</returns>
    public static SparkDeskDrawExecutionSettings FromExecutionSettings(DrawExecutionSettings? executionSettings)
    {
        if (executionSettings is SparkDeskDrawExecutionSettings settings)
        {
            return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        var openAIDrawExecutionSettings = JsonSerializer.Deserialize<SparkDeskDrawExecutionSettings>(json, JsonOptionsCache.ReadPermissive);
        if (openAIDrawExecutionSettings is not null)
        {
            return openAIDrawExecutionSettings;
        }

        throw new JsonException("Failed to convert execution settings to SparkDeskDrawExecutionSettings.");
    }
}
