// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents the metadata associated with a QianFan response.
/// </summary>
public sealed class QianFanMetadata : ReadOnlyDictionary<string, object?>
{
    internal QianFanMetadata() : base(new Dictionary<string, object?>()) { }

    private QianFanMetadata(IDictionary<string, object?> dictionary) : base(dictionary) { }

    /// <summary>
    /// Reason why the processing was finished.
    /// </summary>
    public QianFanFinishReason? FinishReason
    {
        get => this.GetValueFromDictionary(nameof(this.FinishReason)) as QianFanFinishReason?;
        internal init => this.SetValueInDictionary(value, nameof(this.FinishReason));
    }

    /// <summary>
    /// When the response was created.
    /// </summary>
    public long CreatedAt
    {
        get => (this.GetValueFromDictionary(nameof(this.CreatedAt)) as long?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.CreatedAt));
    }

    /// <summary>
    /// The index of the response.
    /// </summary>
    public int SentenceId
    {
        get => (this.GetValueFromDictionary(nameof(this.SentenceId)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.SentenceId));
    }

    public bool IsEnd
    {
        get => (this.GetValueFromDictionary(nameof(this.IsEnd)) as bool?) ?? false;
        internal init => this.SetValueInDictionary(value, nameof(this.IsEnd));
    }

    public bool IsTruncated
    {
        get => (this.GetValueFromDictionary(nameof(this.IsTruncated)) as bool?) ?? false;
        internal init => this.SetValueInDictionary(value, nameof(this.IsTruncated));
    }

    public bool NeedClearHistory
    {
        get => (this.GetValueFromDictionary(nameof(this.NeedClearHistory)) as bool?) ?? false;
        internal init => this.SetValueInDictionary(value, nameof(this.NeedClearHistory));
    }

    public int Flag
    {
        get => (this.GetValueFromDictionary(nameof(this.Flag)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.Flag));
    }

    public int BanRound
    {
        get => (this.GetValueFromDictionary(nameof(this.BanRound)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.BanRound));
    }

    public int PromptTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.PromptTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.PromptTokenCount));
    }

    public int CompletionTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.CompletionTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.CompletionTokenCount));
    }

    public int TotalTokenCount
    {
        get => (this.GetValueFromDictionary(nameof(this.TotalTokenCount)) as int?) ?? 0;
        internal init => this.SetValueInDictionary(value, nameof(this.TotalTokenCount));
    }

    /// <summary>
    /// Converts a dictionary to a <see cref="QianFanMetadata"/> object.
    /// </summary>
    public static QianFanMetadata FromDictionary(IReadOnlyDictionary<string, object?> dictionary) => dictionary switch
    {
        null => throw new ArgumentNullException(nameof(dictionary)),
        QianFanMetadata metadata => metadata,
        IDictionary<string, object?> metadata => new QianFanMetadata(metadata),
        _ => new QianFanMetadata(dictionary.ToDictionary(pair => pair.Key, pair => pair.Value))
    };

    private void SetValueInDictionary(object? value, string propertyName)
        => this.Dictionary[propertyName] = value;

    private object? GetValueFromDictionary(string propertyName)
        => this.Dictionary.TryGetValue(propertyName, out var value) ? value : null;
}
