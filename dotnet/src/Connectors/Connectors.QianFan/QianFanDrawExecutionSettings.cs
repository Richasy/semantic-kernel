// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents the settings for the QianFan draw execution.
/// </summary>
public sealed class QianFanDrawExecutionSettings : DrawExecutionSettings
{
    private int _width = 512;
    private int _height = 512;
    private int _number = 1;
    private string? _negativePrompt;

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

    /// <summary>
    /// Number of images to generate.
    /// </summary>
    [JsonPropertyName("n")]
    public int Number
    {
        get => this._number;
        set
        {
            this.ThrowIfFrozen();
            this._number = value;
        }
    }

    /// <summary>
    /// Negative prompt.
    /// </summary>
    [JsonPropertyName("negative_prompt")]
    public string? NegativePrompt
    {
        get => this._negativePrompt;
        set
        {
            this.ThrowIfFrozen();
            this._negativePrompt = value;
        }
    }

    /// <inheritdoc/>
    public override DrawExecutionSettings Clone()
    {
        return new QianFanDrawExecutionSettings
        {
            Width = this.Width,
            Height = this.Height,
            Number = this.Number,
            NegativePrompt = this.NegativePrompt,
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }

    /// <summary>
    /// Create a new settings object with the values from another settings object.
    /// </summary>
    /// <param name="executionSettings">Template configuration</param>
    /// <returns>An instance of QianFanDrawExecutionSettings</returns>
    public static QianFanDrawExecutionSettings FromExecutionSettings(DrawExecutionSettings? executionSettings)
    {
        if (executionSettings is QianFanDrawExecutionSettings settings)
        {
            return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        var openAIDrawExecutionSettings = JsonSerializer.Deserialize<QianFanDrawExecutionSettings>(json, JsonOptionsCache.ReadPermissive);
        if (openAIDrawExecutionSettings is not null)
        {
            return openAIDrawExecutionSettings;
        }

        throw new JsonException("Failed to convert execution settings to QianFanDrawExecutionSettings.");
    }
}
