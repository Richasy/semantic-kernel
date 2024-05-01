// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.Anthropic;

/// <summary>
/// Represents a Anthropic Stop Reason.
/// </summary>
[JsonConverter(typeof(AnthropicStopReasonConverter))]
public readonly struct AnthropicStopReason : IEquatable<AnthropicStopReason>
{
    /// <summary>
    /// Default value. This value is unused.
    /// </summary>
    public static AnthropicStopReason EndReturn { get; } = new("end_return");

    /// <summary>
    /// Natural stop point of the model or provided stop sequence.
    /// </summary>
    public static AnthropicStopReason Stop { get; } = new("stop_sequence");

    /// <summary>
    /// The maximum number of tokens as specified in the request was reached.
    /// </summary>
    public static AnthropicStopReason MaxTokens { get; } = new("max_tokens");

    /// <summary>
    /// Gets the label of the property.
    /// Label is used for serialization.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Represents a Anthropic Stop Reason.
    /// </summary>
    [JsonConstructor]
    public AnthropicStopReason(string label)
    {
        Verify.NotNullOrWhiteSpace(label, nameof(label));
        this.Label = label;
    }

    /// <summary>
    /// Represents the equality operator for comparing two instances of <see cref="AnthropicStopReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="AnthropicStopReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="AnthropicStopReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AnthropicStopReason left, AnthropicStopReason right)
        => left.Equals(right);

    /// <summary>
    /// Represents the inequality operator for comparing two instances of <see cref="AnthropicStopReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="AnthropicStopReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="AnthropicStopReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AnthropicStopReason left, AnthropicStopReason right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(AnthropicStopReason other)
        => string.Equals(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is AnthropicStopReason other && this == other;

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Label ?? string.Empty);

    /// <inheritdoc />
    public override string ToString() => this.Label ?? string.Empty;
}

internal sealed class AnthropicStopReasonConverter : JsonConverter<AnthropicStopReason>
{
    public override AnthropicStopReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, AnthropicStopReason value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Label);
}
