// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

/// <summary>
/// Represents the metadata associated with a Anthropic response.
/// </summary>
public sealed class AnthropicMetadata : ReadOnlyDictionary<string, object?>
{
    internal AnthropicMetadata() : base(new Dictionary<string, object?>()) { }

    private AnthropicMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// Reason why the processing was finished.
    /// </summary>
    public AnthropicStopReason? StopReason
    {
        get => this.GetValueFromDictionary(nameof(this.StopReason)) as AnthropicStopReason?;
        internal init => this.SetValueInDictionary(value, nameof(this.StopReason));
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
    /// The count of token in the current output.
    /// </summary>
    public int OutputTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.OutputTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.OutputTokenCount));
    }

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
