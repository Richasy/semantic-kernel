// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.DouBao;

/// <summary>
/// Represents a DouBao Finish Reason.
/// </summary>
[JsonConverter(typeof(DouBaoFinishReasonConverter))]
public readonly struct DouBaoFinishReason : IEquatable<DouBaoFinishReason>
{
    /// <summary>
    /// Natural stop point of the model or provided stop sequence.
    /// </summary>
    public static DouBaoFinishReason Stop { get; } = new("stop");

    /// <summary>
    /// The candidate content was flagged for safety reasons.
    /// </summary>
    public static DouBaoFinishReason MaxLength { get; } = new("length");

    /// <summary>
    /// The candidate content was flagged for safety reasons.
    /// </summary>
    public static DouBaoFinishReason ContentFilter { get; } = new("content_filter");

    /// <summary>
    /// Gets the label of the property.
    /// Label is used for serialization.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Represents a DouBao Finish Reason.
    /// </summary>
    [JsonConstructor]
    public DouBaoFinishReason(string label)
    {
        Verify.NotNullOrWhiteSpace(label, nameof(label));
        this.Label = label;
    }

    /// <summary>
    /// Represents the equality operator for comparing two instances of <see cref="DouBaoFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="DouBaoFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="DouBaoFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DouBaoFinishReason left, DouBaoFinishReason right)
        => left.Equals(right);

    /// <summary>
    /// Represents the inequality operator for comparing two instances of <see cref="DouBaoFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="DouBaoFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="DouBaoFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DouBaoFinishReason left, DouBaoFinishReason right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(DouBaoFinishReason other)
        => string.Equals(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is DouBaoFinishReason other && this == other;

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Label ?? string.Empty);

    /// <inheritdoc />
    public override string ToString() => this.Label ?? string.Empty;
}

internal sealed class DouBaoFinishReasonConverter : JsonConverter<DouBaoFinishReason>
{
    public override DouBaoFinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DouBaoFinishReason value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Label);
}
