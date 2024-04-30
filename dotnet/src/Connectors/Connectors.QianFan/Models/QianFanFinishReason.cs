// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.QianFan;

/// <summary>
/// Represents a QianFan Finish Reason.
/// </summary>
[JsonConverter(typeof(QianFanFinishReasonConverter))]
public readonly struct QianFanFinishReason : IEquatable<QianFanFinishReason>
{
    /// <summary>
    /// Default value. This value is unused.
    /// </summary>
    public static QianFanFinishReason Normal { get; } = new("normal");

    /// <summary>
    /// Natural stop point of the model or provided stop sequence.
    /// </summary>
    public static QianFanFinishReason Stop { get; } = new("stop");

    /// <summary>
    /// The maximum number of tokens as specified in the request was reached.
    /// </summary>
    public static QianFanFinishReason MaxTokens { get; } = new("length");

    /// <summary>
    /// The candidate content was flagged for safety reasons.
    /// </summary>
    public static QianFanFinishReason Filter { get; } = new("content_filter");

    /// <summary>
    /// Gets the label of the property.
    /// Label is used for serialization.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Represents a QianFan Finish Reason.
    /// </summary>
    [JsonConstructor]
    public QianFanFinishReason(string label)
    {
        Verify.NotNullOrWhiteSpace(label, nameof(label));
        this.Label = label;
    }

    /// <summary>
    /// Represents the equality operator for comparing two instances of <see cref="QianFanFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="QianFanFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="QianFanFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(QianFanFinishReason left, QianFanFinishReason right)
        => left.Equals(right);

    /// <summary>
    /// Represents the inequality operator for comparing two instances of <see cref="QianFanFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="QianFanFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="QianFanFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(QianFanFinishReason left, QianFanFinishReason right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(QianFanFinishReason other)
        => string.Equals(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is QianFanFinishReason other && this == other;

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Label ?? string.Empty);

    /// <inheritdoc />
    public override string ToString() => this.Label ?? string.Empty;
}

internal sealed class QianFanFinishReasonConverter : JsonConverter<QianFanFinishReason>
{
    public override QianFanFinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, QianFanFinishReason value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Label);
}
