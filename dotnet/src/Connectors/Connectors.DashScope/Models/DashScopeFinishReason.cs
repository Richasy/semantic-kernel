// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Represents a DashScope Finish Reason.
/// </summary>
[JsonConverter(typeof(DashScopeFinishReasonConverter))]
public readonly struct DashScopeFinishReason : IEquatable<DashScopeFinishReason>
{
    /// <summary>
    /// Default value. This value is unused.
    /// </summary>
    public static DashScopeFinishReason Unspecified { get; } = new("null");

    /// <summary>
    /// Natural stop point of the model or provided stop sequence.
    /// </summary>
    public static DashScopeFinishReason Stop { get; } = new("stop");

    /// <summary>
    /// The maximum number of tokens as specified in the request was reached.
    /// </summary>
    public static DashScopeFinishReason MaxTokens { get; } = new("length");

    /// <summary>
    /// The maximum number of tokens as specified in the request was reached.
    /// </summary>
    public static DashScopeFinishReason ToolCalls { get; } = new("tool_calls");

    /// <summary>
    /// Gets the label of the property.
    /// Label is used for serialization.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Represents a DashScope Finish Reason.
    /// </summary>
    [JsonConstructor]
    public DashScopeFinishReason(string label)
    {
        Verify.NotNullOrWhiteSpace(label, nameof(label));
        this.Label = label;
    }

    /// <summary>
    /// Represents the equality operator for comparing two instances of <see cref="DashScopeFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="DashScopeFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="DashScopeFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(DashScopeFinishReason left, DashScopeFinishReason right)
        => left.Equals(right);

    /// <summary>
    /// Represents the inequality operator for comparing two instances of <see cref="DashScopeFinishReason"/>.
    /// </summary>
    /// <param name="left">The left <see cref="DashScopeFinishReason"/> instance to compare.</param>
    /// <param name="right">The right <see cref="DashScopeFinishReason"/> instance to compare.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(DashScopeFinishReason left, DashScopeFinishReason right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(DashScopeFinishReason other)
        => string.Equals(this.Label, other.Label, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is DashScopeFinishReason other && this == other;

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(this.Label ?? string.Empty);

    /// <inheritdoc />
    public override string ToString() => this.Label ?? string.Empty;
}

internal sealed class DashScopeFinishReasonConverter : JsonConverter<DashScopeFinishReason>
{
    public override DashScopeFinishReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DashScopeFinishReason value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Label);
}
