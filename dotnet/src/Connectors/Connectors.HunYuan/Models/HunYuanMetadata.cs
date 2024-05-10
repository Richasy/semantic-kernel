// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Represents the metadata associated with a HunYuan response.
/// </summary>
public sealed class HunYuanMetadata : ReadOnlyDictionary<string, object?>
{
    internal HunYuanMetadata() : base(new Dictionary<string, object?>()) { }

    private HunYuanMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// Reason why the processing was finished.
    /// </summary>
    public HunYuanFinishReason? FinishReason
    {
        get => this.GetValueFromDictionary(nameof(this.FinishReason)) as HunYuanFinishReason?;
        internal init => this.SetValueInDictionary(value, nameof(this.FinishReason));
    }

    /// <summary>
    /// The error message.
    /// </summary>
    public string? ErrorMessage
    {
        get => (this.GetValueFromDictionary(nameof(this.ErrorMessage)) as string) ?? default;
        internal init => this.SetValueInDictionary(value, nameof(this.ErrorMessage));
    }

    /// <summary>
    /// The token count of the prompt.
    /// </summary>
    public int PromptTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.PromptTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.PromptTokenCount));
    }

    /// <summary>
    /// The token count of the response.
    /// </summary>
    public int CompletionTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.CompletionTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.CompletionTokenCount));
    }

    /// <summary>
    /// The total token count.
    /// </summary>
    public int TotalTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.TotalTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.TotalTokenCount));
    }

    /// <summary>
    /// Converts a dictionary to a <see cref="HunYuanMetadata"/> object.
    /// </summary>
    public static HunYuanMetadata FromDictionary(IReadOnlyDictionary<string, object?> dictionary) => dictionary switch
    {
        null => throw new ArgumentNullException(nameof(dictionary)),
        HunYuanMetadata metadata => metadata,
        IDictionary<string, object?> metadata => new HunYuanMetadata(metadata),
        _ => new HunYuanMetadata(dictionary.ToDictionary(pair => pair.Key, pair => pair.Value))
    };

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
