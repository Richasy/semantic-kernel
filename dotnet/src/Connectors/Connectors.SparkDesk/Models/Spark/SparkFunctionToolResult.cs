// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Represents the result of a Spark function tool call.
/// </summary>
public sealed class SparkFunctionToolResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkFunctionToolResult"/> class.
    /// </summary>
    /// <param name="toolCall">The called function.</param>
    /// <param name="functionResult">The result of the function.</param>
    public SparkFunctionToolResult(SparkFunctionToolCall toolCall, FunctionResult functionResult)
    {
        Verify.NotNull(toolCall);
        Verify.NotNull(functionResult);

        this.FunctionResult = functionResult;
        this.FullyQualifiedName = toolCall.FullyQualifiedName;
    }

    /// <summary>
    /// Gets the result of the function.
    /// </summary>
    public FunctionResult FunctionResult { get; }

    /// <summary>Gets the fully-qualified name of the function.</summary>
    /// <seealso cref="SparkFunctionToolCall.FullyQualifiedName">SparkFunctionToolCall.FullyQualifiedName</seealso>
    public string FullyQualifiedName { get; }
}
