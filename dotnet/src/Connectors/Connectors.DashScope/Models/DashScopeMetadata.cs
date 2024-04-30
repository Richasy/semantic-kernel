// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Represents the metadata associated with a DashScope response.
/// </summary>
public sealed class DashScopeMetadata : ReadOnlyDictionary<string, object?>
{
    internal DashScopeMetadata() : base(new Dictionary<string, object?>()) { }

    private DashScopeMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// Reason why the processing was finished.
    /// </summary>
    public DashScopeFinishReason? FinishReason
    {
        get => this.GetValueFromDictionary(nameof(this.FinishReason)) as DashScopeFinishReason?;
        internal init => this.SetValueInDictionary(value, nameof(this.FinishReason));
    }

    /// <summary>
    /// The count of tokens in the prompt.
    /// </summary>
    public int InputTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.InputTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.InputTokenCount));
    }

    /// <summary>
    /// The count of token in the current candidate.
    /// </summary>
    public int OutputTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.OutputTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.OutputTokenCount));
    }

    /// <summary>
    /// The total count of tokens (prompt + total candidates token count).
    /// </summary>
    public int TotalTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.TotalTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.TotalTokenCount));
    }

    /// <summary>
    /// Converts a dictionary to a <see cref="DashScopeMetadata"/> object.
    /// </summary>
    public static DashScopeMetadata FromDictionary(IReadOnlyDictionary<string, object?> dictionary) => dictionary switch
    {
        null => throw new ArgumentNullException(nameof(dictionary)),
        DashScopeMetadata metadata => metadata,
        IDictionary<string, object?> metadata => new DashScopeMetadata(metadata),
        _ => new DashScopeMetadata(dictionary.ToDictionary(pair => pair.Key, pair => pair.Value))
    };

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
