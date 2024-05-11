// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp;

/// <summary>
/// Represents the settings for executing a prompt with the local model.
/// </summary>
public sealed class LlamaSharpPromptExecutionSettings : PromptExecutionSettings
{
    private const string DEFAULT_END_SUFFIX = "<|im_end|>";
    private double _temperature = 1;
    private double _topP = 1;
    private double _presencePenalty = 0;
    private double _frequencyPenalty = 0;
    private int? _maxTokens;
    private IList<string>? _stopSequences = [DEFAULT_END_SUFFIX];
    private int _resultsPerPrompt = 1;
    private string? _responseFormat = string.Empty;
    private IDictionary<int, int>? _tokenSelectionBiases = new Dictionary<int, int>();

    /// <summary>
    /// Temperature controls the randomness of the completion.
    /// The higher the temperature, the more random the completion.
    /// Default is 1.0.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double Temperature
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
    /// Default is 1.0.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double TopP
    {
        get => this._topP;

        set
        {
            this.ThrowIfFrozen();
            this._topP = value;
        }
    }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens
    /// based on whether they appear in the text so far, increasing the
    /// model's likelihood to talk about new topics.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty
    {
        get => this._presencePenalty;

        set
        {
            this.ThrowIfFrozen();
            this._presencePenalty = value;
        }
    }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens
    /// based on their existing frequency in the text so far, decreasing
    /// the model's likelihood to repeat the same line verbatim.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty
    {
        get => this._frequencyPenalty;

        set
        {
            this.ThrowIfFrozen();
            this._frequencyPenalty = value;
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
            this._maxTokens = value;
        }
    }

    /// <summary>
    /// Sequences where the completion will stop generating further tokens.
    /// </summary>
    [JsonPropertyName("stop_sequences")]
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
    /// How many completions to generate for each prompt. Default is 1.
    /// Note: Because this parameter generates many completions, it can quickly consume your token quota.
    /// Use carefully and ensure that you have reasonable settings for max_tokens and stop.
    /// </summary>
    [JsonPropertyName("results_per_prompt")]
    public int ResultsPerPrompt
    {
        get => this._resultsPerPrompt;

        set
        {
            this.ThrowIfFrozen();
            this._resultsPerPrompt = value;
        }
    }

    /// <summary>
    /// Indicates the format of the response which can be used downstream to post-process the messages. Handlebars: handlebars_object. JSON: json_object, etc.
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
    /// Modify the likelihood of specified tokens appearing in the completion.
    /// </summary>
    [JsonPropertyName("token_selection_biases")]
    public IDictionary<int, int>? TokenSelectionBiases
    {
        get => this._tokenSelectionBiases;

        set
        {
            this.ThrowIfFrozen();
            this._tokenSelectionBiases = value;
        }
    }

    /// <inheritdoc/>
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

        if (this._tokenSelectionBiases is not null)
        {
            this._tokenSelectionBiases = new ReadOnlyDictionary<int, int>(this._tokenSelectionBiases);
        }
    }

    /// <inheritdoc/>
    public override PromptExecutionSettings Clone()
    {
        return new LlamaSharpPromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            PresencePenalty = this.PresencePenalty,
            FrequencyPenalty = this.FrequencyPenalty,
            MaxTokens = this.MaxTokens,
            StopSequences = this.StopSequences is not null ? new List<string>(this.StopSequences) : null,
            ResultsPerPrompt = this.ResultsPerPrompt,
            ResponseFormat = this.ResponseFormat,
            TokenSelectionBiases = this.TokenSelectionBiases is not null ? new Dictionary<int, int>(this.TokenSelectionBiases) : null,
        };
    }

    /// <summary>
    /// Default max tokens for a text generation
    /// </summary>
    internal static int DefaultTextMaxTokens { get; } = 256;

    /// <summary>
    /// Create a new settings object with the values from another settings object.
    /// </summary>
    /// <param name="executionSettings">Template configuration</param>
    /// <param name="defaultMaxTokens">Default max tokens</param>
    /// <returns>An instance of LlamaSharpPromptExecutionSettings</returns>
    public static LlamaSharpPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings, int? defaultMaxTokens = null)
    {
        if (executionSettings is null)
        {
            return new LlamaSharpPromptExecutionSettings()
            {
                MaxTokens = defaultMaxTokens
            };
        }

        if (executionSettings is LlamaSharpPromptExecutionSettings settings)
        {
            return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);

        var LlamaSharpExecutionSettings = JsonSerializer.Deserialize<LlamaSharpPromptExecutionSettings>(json, s_options);
        if (LlamaSharpExecutionSettings is not null)
        {
            return LlamaSharpExecutionSettings;
        }

        throw new ArgumentException($"Invalid execution settings, cannot convert to {nameof(LlamaSharpPromptExecutionSettings)}", nameof(executionSettings));
    }

    private static readonly JsonSerializerOptions s_options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            MaxDepth = 20,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new PromptExecutionSettingsConverter() }
        };

        return options;
    }
}
