// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Represents the settings for executing a prompt with the HunYuan model.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class HunYuanPromptExecutionSettings : PromptExecutionSettings
{
    private double? _temperature;
    private double? _topP;
    private bool? _streamModeration;

    /// <summary>
    /// Temperature controls the randomness of the completion.
    /// The higher the temperature, the more random the completion.
    /// Range is 0.0 to 1.0.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature
    {
        get => this._temperature;
        set
        {
            this.ThrowIfFrozen();
            this._temperature = value;
        }
    }

    /// <summary>
    /// TopP controls the diversity of the completion.
    /// The higher the TopP, the more diverse the completion.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP
    {
        get => this._topP;
        set
        {
            this.ThrowIfFrozen();
            this._topP = value;
        }
    }

    /// <summary>
    /// Streaming output audit switch.
    /// </summary>
    [JsonPropertyName("stream_moderation")]
    public bool? StreamModeration
    {
        get => this._streamModeration;
        set
        {
            this.ThrowIfFrozen();
            this._streamModeration = value;
        }
    }

    /// <inheritdoc />
    public override PromptExecutionSettings Clone()
    {
        return new HunYuanPromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            StreamModeration = this.StreamModeration,
        };
    }

    /// <summary>
    /// Converts a <see cref="PromptExecutionSettings"/> object to a <see cref="HunYuanPromptExecutionSettings"/> object.
    /// </summary>
    /// <param name="executionSettings">The <see cref="PromptExecutionSettings"/> object to convert.</param>
    /// <returns>
    /// The converted <see cref="HunYuanPromptExecutionSettings"/> object. If <paramref name="executionSettings"/> is null,
    /// a new instance of <see cref="HunYuanPromptExecutionSettings"/> is returned. If <paramref name="executionSettings"/>
    /// is already a <see cref="HunYuanPromptExecutionSettings"/> object, it is casted and returned. Otherwise, the method
    /// tries to deserialize <paramref name="executionSettings"/> to a <see cref="HunYuanPromptExecutionSettings"/> object.
    /// If deserialization is successful, the converted object is returned. If deserialization fails or the converted object
    /// is null, an <see cref="ArgumentException"/> is thrown.
    /// </returns>
    public static HunYuanPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings)
    {
        switch (executionSettings)
        {
            case null:
                return new HunYuanPromptExecutionSettings();
            case HunYuanPromptExecutionSettings settings:
                return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        return JsonSerializer.Deserialize<HunYuanPromptExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }
}
