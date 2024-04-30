// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Represents the metadata associated with a Spark response.
/// </summary>
public sealed class SparkMetadata : ReadOnlyDictionary<string, object?>
{
    internal SparkMetadata() : base(new Dictionary<string, object?>()) { }

    private SparkMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// The count of tokens in the question.
    /// </summary>
    public int QuestionTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.QuestionTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.QuestionTokenCount));
    }

    /// <summary>
    /// The total size of tokens that contains historical issues.
    /// </summary>
    public int PromptTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.PromptTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.PromptTokenCount));
    }

    /// <summary>
    /// The count of tokens in the completion.
    /// </summary>
    public int CompletionTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.CompletionTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.CompletionTokenCount));
    }

    /// <summary>
    /// The total count of tokens of the all candidate responses.
    /// </summary>
    public int TotalTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.TotalTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.TotalTokenCount));
    }

    /// <summary>
    /// The status of the Spark response.
    /// </summary>
    public int Status
    {
        get => (this.GetValueFromDictionary(nameof(this.Status)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.Status));
    }

    /// <summary>
    /// Convert a dictionary to a <see cref="SparkMetadata"/>.
    /// </summary>
    public static SparkMetadata FromDictionary(IReadOnlyDictionary<string, object?> dictionary) => dictionary switch
    {
        null => throw new System.ArgumentNullException(nameof(dictionary)),
        SparkMetadata metadata => metadata,
        IDictionary<string, object?> metadata => new SparkMetadata(metadata),
        _ => new SparkMetadata(dictionary.ToDictionary(pair => pair.Key, pair => pair.Value))
    };

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
