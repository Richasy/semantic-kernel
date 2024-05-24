// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents the settings for executing a prompt with the QianFan model.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class QianFanPromptExecutionSettings : PromptExecutionSettings
{
    private double? _temperature;
    private double? _topP;
    private double? _penaltyScore;
    private string? _responseFormat;
    private int? _maxTokens;
    private IList<string>? _stopSequences;
    private bool? _disableSearch;
    private bool? _enableCitation;
    private bool? _enableTrace;

    /// <summary>
    /// Default max tokens for a text generation.
    /// </summary>
    public static int DefaultTextMaxTokens { get; } = 512;

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
    /// The maximum number of tokens to generate in the completion.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens
    {
        get => this._maxTokens;
        set
        {
            this.ThrowIfFrozen();
            this._maxTokens = value == null || value <= 2 ? default : value;
        }
    }

    /// <summary>
    /// Reduce the phenomenon of duplicate generation by increasing penalties for generated tokens.
    /// </summary>
    [JsonPropertyName("penalty_score")]
    public double? PenaltyScore
    {
        get => this._penaltyScore;
        set
        {
            this.ThrowIfFrozen();
            this._penaltyScore = value;
        }
    }

    /// <summary>
    /// Specifies the format of the response content. <c>text</c> or <c>json_object</c>.
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat
    {
        get => this._responseFormat;
        set
        {
            this.ThrowIfFrozen();
            this._responseFormat = value;
        }
    }

    /// <summary>
    /// Sequences where the completion will stop generating further tokens.
    /// Maximum number of stop sequences is 4.
    /// </summary>
    [JsonPropertyName("stop")]
    public IList<string>? StopSequences
    {
        get => this._stopSequences;
        set
        {
            this.ThrowIfFrozen();
            this._stopSequences = value;
        }
    }

    /// <summary>
    /// Whether to forcibly disable the real-time search function, which is false by default, indicates that it is not disabled.
    /// </summary>
    [JsonPropertyName("disable_search")]
    public bool? DisableSearch
    {
        get => this._disableSearch;
        set
        {
            this.ThrowIfFrozen();
            this._disableSearch = value;
        }
    }

    /// <summary>
    /// Whether to enable the upper corner mark to return.
    /// </summary>
    [JsonPropertyName("enable_citation")]
    public bool? EnableCitation
    {
        get => this._enableCitation;
        set
        {
            this.ThrowIfFrozen();
            this._enableCitation = value;
        }
    }

    /// <summary>
    /// Whether to enable the trace function.
    /// </summary>
    [JsonPropertyName("enable_trace")]
    public bool? EnableTrace
    {
        get => this._enableTrace;
        set
        {
            this.ThrowIfFrozen();
            this._enableTrace = value;
        }
    }

    /// <inheritdoc />
    public override void Freeze()
    {
        if (this.IsFrozen)
        {
            return;
        }

        base.Freeze();

        if (this._stopSequences is not null)
        {
            this._stopSequences = new ReadOnlyCollection<string>(this._stopSequences);
        }
    }

    /// <inheritdoc />
    public override PromptExecutionSettings Clone()
    {
        return new QianFanPromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            MaxTokens = this.MaxTokens,
            PenaltyScore = this.PenaltyScore,
            ResponseFormat = this.ResponseFormat,
            DisableSearch = this.DisableSearch,
            EnableCitation = this.EnableCitation,
            StopSequences = this.StopSequences is not null ? new List<string>(this.StopSequences) : null,
        };
    }

    /// <summary>
    /// Converts a <see cref="PromptExecutionSettings"/> object to a <see cref="QianFanPromptExecutionSettings"/> object.
    /// </summary>
    /// <param name="executionSettings">The <see cref="PromptExecutionSettings"/> object to convert.</param>
    /// <returns>
    /// The converted <see cref="QianFanPromptExecutionSettings"/> object. If <paramref name="executionSettings"/> is null,
    /// a new instance of <see cref="QianFanPromptExecutionSettings"/> is returned. If <paramref name="executionSettings"/>
    /// is already a <see cref="QianFanPromptExecutionSettings"/> object, it is casted and returned. Otherwise, the method
    /// tries to deserialize <paramref name="executionSettings"/> to a <see cref="QianFanPromptExecutionSettings"/> object.
    /// If deserialization is successful, the converted object is returned. If deserialization fails or the converted object
    /// is null, an <see cref="ArgumentException"/> is thrown.
    /// </returns>
    public static QianFanPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings)
    {
        switch (executionSettings)
        {
            case null:
                return new QianFanPromptExecutionSettings() { MaxTokens = DefaultTextMaxTokens };
            case QianFanPromptExecutionSettings settings:
                return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        return JsonSerializer.Deserialize<QianFanPromptExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }
}
