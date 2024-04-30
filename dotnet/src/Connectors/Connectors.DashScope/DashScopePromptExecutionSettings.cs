// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Represents the settings for executing a prompt with the DashScope model.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class DashScopePromptExecutionSettings : PromptExecutionSettings
{
    private double? _temperature;
    private double? _topP;
    private int? _topK;
    private int? _maxTokens;
    private int? _seed;
    private double? _repetitionPenalty;
    private IList<string>? _stopSequences;
    private bool? _enableSearch;
    private string? _resultFormat = "message";
    private DashScopeToolCallBehavior? _toolCallBehavior;

    /// <summary>
    /// Default max tokens for a text generation.
    /// </summary>
    public static int DefaultTextMaxTokens { get; } = 256;

    /// <summary>
    /// Temperature controls the randomness of the completion.
    /// The higher the temperature, the more random the completion.
    /// Range is 0.0 to 1.0.
    /// </summary>
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
    /// Gets or sets the value of the TopK property.
    /// The TopK property represents the maximum value of a collection or dataset.
    /// </summary>
    public int? TopK
    {
        get => this._topK;
        set
        {
            this.ThrowIfFrozen();
            this._topK = value;
        }
    }

    /// <summary>
    /// The maximum number of tokens to generate in the completion.
    /// </summary>
    public int? MaxTokens
    {
        get => this._maxTokens;
        set
        {
            this.ThrowIfFrozen();
            this._maxTokens = value;
        }
    }

    public int? Seed
    {
        get => this._seed;
        set
        {
            this.ThrowIfFrozen();
            this._seed = value;
        }
    }

    public double? RepetitionPenalty
    {
        get => this._repetitionPenalty;
        set
        {
            this.ThrowIfFrozen();
            this._repetitionPenalty = value;
        }
    }

    public IList<string>? StopSequences
    {
        get => this._stopSequences;
        set
        {
            this.ThrowIfFrozen();
            this._stopSequences = value;
        }
    }

    public bool? EnableSearch
    {
        get => this._enableSearch;
        set
        {
            this.ThrowIfFrozen();
            this._enableSearch = value;
        }
    }

    public string? ResultFormat
    {
        get => this._resultFormat;
        set
        {
            this.ThrowIfFrozen();
            this._resultFormat = value;
        }
    }

    /// <summary>
    /// Gets or sets the behavior for how tool calls are handled.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>To disable all tool calling, set the property to null (the default).</item>
    /// <item>
    /// To allow the model to request one of any number of functions, set the property to an
    /// instance returned from <see cref="DashScopeToolCallBehavior.EnableFunctions"/>, called with
    /// a list of the functions available.
    /// </item>
    /// <item>
    /// To allow the model to request one of any of the functions in the supplied <see cref="Kernel"/>,
    /// set the property to <see cref="DashScopeToolCallBehavior.EnableKernelFunctions"/> if the client should simply
    /// send the information about the functions and not handle the response in any special manner, or
    /// <see cref="DashScopeToolCallBehavior.AutoInvokeKernelFunctions"/> if the client should attempt to automatically
    /// invoke the function and send the result back to the service.
    /// </item>
    /// </list>
    /// For all options where an instance is provided, auto-invoke behavior may be selected. If the service
    /// sends a request for a function call, if auto-invoke has been requested, the client will attempt to
    /// resolve that function from the functions available in the <see cref="Kernel"/>, and if found, rather
    /// than returning the response back to the caller, it will handle the request automatically, invoking
    /// the function, and sending back the result. The intermediate messages will be retained in the
    /// <see cref="ChatHistory"/> if an instance was provided.
    /// </remarks>
    public DashScopeToolCallBehavior? ToolCallBehavior
    {
        get => this._toolCallBehavior;

        set
        {
            this.ThrowIfFrozen();
            this._toolCallBehavior = value;
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
        return new DashScopePromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            TopK = this.TopK,
            MaxTokens = this.MaxTokens,
            Seed = this.Seed,
            RepetitionPenalty = this.RepetitionPenalty,
            EnableSearch = this.EnableSearch,
            StopSequences = this.StopSequences is not null ? new List<string>(this.StopSequences) : null,
            ToolCallBehavior = this.ToolCallBehavior?.Clone(),
        };
    }

    /// <summary>
    /// Converts a <see cref="PromptExecutionSettings"/> object to a <see cref="DashScopePromptExecutionSettings"/> object.
    /// </summary>
    /// <param name="executionSettings">The <see cref="PromptExecutionSettings"/> object to convert.</param>
    /// <returns>
    /// The converted <see cref="DashScopePromptExecutionSettings"/> object. If <paramref name="executionSettings"/> is null,
    /// a new instance of <see cref="DashScopePromptExecutionSettings"/> is returned. If <paramref name="executionSettings"/>
    /// is already a <see cref="DashScopePromptExecutionSettings"/> object, it is casted and returned. Otherwise, the method
    /// tries to deserialize <paramref name="executionSettings"/> to a <see cref="DashScopePromptExecutionSettings"/> object.
    /// If deserialization is successful, the converted object is returned. If deserialization fails or the converted object
    /// is null, an <see cref="ArgumentException"/> is thrown.
    /// </returns>
    public static DashScopePromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings)
    {
        switch (executionSettings)
        {
            case null:
                return new DashScopePromptExecutionSettings() { MaxTokens = DefaultTextMaxTokens };
            case DashScopePromptExecutionSettings settings:
                return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);
        return JsonSerializer.Deserialize<DashScopePromptExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }
}
