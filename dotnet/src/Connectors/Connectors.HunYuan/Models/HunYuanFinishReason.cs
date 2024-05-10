// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.HunYuan;

/// <summary>
/// Represents a HunYuan Finish Reason.
/// </summary>
[JsonConverter(typeof(HunYuanFinishReasonConverter))]
public readonly struct HunYuanFinishReason : IEquatable<HunYuanFinishReason>
{
    /// <summary>
    /// Natural stop point of the model or provided stop sequence.
    /// </summary>
    public static HunYuanFinishReason Stop { get; } = new("stop");

    /// <summary>
    /// The candidate content was flagged for safety reasons.
    /// </summary>
    public static HunYuanFinishReason Sensitive { get; } = new("sensitive");

    /// <summary>
    /// Gets the label of the property.
    /// Label is used for serialization.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Represents a HunYuan Finish Reason.
    /// </summary>
    [JsonConstructor]
    public HunYuanFinishReason(string label)
    {
        Verify.NotNullOrWhiteSpace(label, nameof(label));
        this.Label = label;
    }

    /// <summary>
    /// Represents the equality operator for comparing two instances of <see cref="HunYuanFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="HunYuanFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="HunYuanFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(HunYuanFinishReason left, HunYuanFinishReason right)
        => left.Equals(right);

    /// <summary>
    /// Represents the inequality operator for comparing two instances of <see cref="HunYuanFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="HunYuanFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="HunYuanFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(HunYuanFinishReason left, HunYuanFinishReason right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(HunYuanFinishReason other)
        => string.Equals(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is HunYuanFinishReason other && this == other;

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Label ?? string.Empty);

    /// <inheritdoc />
    public override string ToString() => this.Label ?? string.Empty;
}

internal sealed class HunYuanFinishReasonConverter : JsonConverter<HunYuanFinishReason>
{
    public override HunYuanFinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, HunYuanFinishReason value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Label);
}
