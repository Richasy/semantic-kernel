// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.DouBao;

/// <summary>
/// Represents the metadata associated with a DouBao response.
/// </summary>
public sealed class DouBaoMetadata : ReadOnlyDictionary<string, object?>
{
    internal DouBaoMetadata() : base(new Dictionary<string, object?>()) { }

    private DouBaoMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// Reason why the processing was finished.
    /// </summary>
    public DouBaoFinishReason? FinishReason
    {
        get => this.GetValueFromDictionary(nameof(this.FinishReason)) as DouBaoFinishReason?;
        internal init => this.SetValueInDictionary(value, nameof(this.FinishReason));
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
    /// Converts a dictionary to a <see cref="DouBaoMetadata"/> object.
    /// </summary>
    public static DouBaoMetadata FromDictionary(IReadOnlyDictionary<string, object?> dictionary) => dictionary switch
    {
        null => throw new ArgumentNullException(nameof(dictionary)),
        DouBaoMetadata metadata => metadata,
        IDictionary<string, object?> metadata => new DouBaoMetadata(metadata),
        _ => new DouBaoMetadata(dictionary.ToDictionary(pair => pair.Key, pair => pair.Value))
    };

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
