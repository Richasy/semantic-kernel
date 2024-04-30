// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Represents a function parameter that can be passed to an SparkDesk function tool call.
/// </summary>
public sealed class SparkFunctionParameter
{
    internal SparkFunctionParameter(
        string? name,
        string? description,
        bool isRequired,
        Type? parameterType,
        KernelJsonSchema? schema)
    {
        this.Name = name ?? string.Empty;
        this.Description = description ?? string.Empty;
        this.IsRequired = isRequired;
        this.ParameterType = parameterType;
        this.Schema = schema;
    }

    /// <summary>Gets the name of the parameter.</summary>
    public string Name { get; }

    /// <summary>Gets a description of the parameter.</summary>
    public string Description { get; }

    /// <summary>Gets whether the parameter is required vs optional.</summary>
    public bool IsRequired { get; }

    /// <summary>Gets the <see cref="Type"/> of the parameter, if known.</summary>
    public Type? ParameterType { get; }

    /// <summary>Gets a JSON schema for the parameter, if known.</summary>
    public KernelJsonSchema? Schema { get; }
}

/// <summary>
/// Represents a function return parameter that can be returned by a tool call to Spark Desk.
/// </summary>
public sealed class SparkFunctionReturnParameter
{
    internal SparkFunctionReturnParameter(string? description, Type? parameterType, KernelJsonSchema? schema)
    {
        this.Description = description ?? string.Empty;
        this.Schema = schema;
        this.ParameterType = parameterType;
    }

    /// <summary>Gets a description of the return parameter.</summary>
    public string Description { get; }

    /// <summary>Gets the <see cref="Type"/> of the return parameter, if known.</summary>
    public Type? ParameterType { get; }

    /// <summary>Gets a JSON schema for the return parameter, if known.</summary>
    public KernelJsonSchema? Schema { get; }
}

/// <summary>
/// Represents a function that can be passed to the Spark API
/// </summary>
public sealed class SparkFunction
{
    /// <summary>
    /// Cached schema for a description less string.
    /// </summary>
    private static readonly KernelJsonSchema s_stringNoDescriptionSchema = KernelJsonSchema.Parse("{\"type\":\"string\"}");

    /// <summary>Initializes the <see cref="SparkFunction"/>.</summary>
    internal SparkFunction(
        string? pluginName,
        string functionName,
        string? description,
        IReadOnlyList<SparkFunctionParameter>? parameters,
        SparkFunctionReturnParameter? returnParameter)
    {
        Verify.NotNullOrWhiteSpace(functionName);

        this.PluginName = pluginName;
        this.FunctionName = functionName;
        this.Description = description;
        this.Parameters = parameters;
        this.ReturnParameter = returnParameter;
    }

    /// <summary>Gets the separator used between the plugin name and the function name, if a plugin name is present.</summary>
    /// <remarks>Default is <c>_</c><br/> It can't be <c>-</c>, because Spark truncates the plugin name if a dash is used</remarks>
    public static string NameSeparator { get; set; } = "_";

    /// <summary>Gets the name of the plugin with which the function is associated, if any.</summary>
    public string? PluginName { get; }

    /// <summary>Gets the name of the function.</summary>
    public string FunctionName { get; }

    /// <summary>Gets the fully-qualified name of the function.</summary>
    /// <remarks>
    /// This is the concatenation of the <see cref="PluginName"/> and the <see cref="FunctionName"/>,
    /// separated by <see cref="NameSeparator"/>. If there is no <see cref="PluginName"/>, this is
    /// the same as <see cref="FunctionName"/>.
    /// </remarks>
    public string FullyQualifiedName =>
        string.IsNullOrEmpty(this.PluginName) ? this.FunctionName : $"{this.PluginName}{NameSeparator}{this.FunctionName}";

    /// <summary>Gets a description of the function.</summary>
    public string? Description { get; }

    /// <summary>Gets a list of parameters to the function, if any.</summary>
    public IReadOnlyList<SparkFunctionParameter>? Parameters { get; }

    /// <summary>Gets the return parameter of the function, if any.</summary>
    public SparkFunctionReturnParameter? ReturnParameter { get; }

    /// <summary>
    /// Converts the <see cref="SparkFunction"/> representation to the Spark API's
    /// <see cref="SparkTool.FunctionDeclaration"/> representation.
    /// </summary>
    /// <returns>A <see cref="SparkTool.FunctionDeclaration"/> containing all the function information.</returns>
    internal SparkTool.FunctionDeclaration ToFunctionDeclaration()
    {
        Dictionary<string, object?>? resultParameters = null;

        if (this.Parameters is { Count: > 0 })
        {
            var properties = new Dictionary<string, KernelJsonSchema>();
            var required = new List<string>();

            foreach (var parameter in this.Parameters)
            {
                properties.Add(parameter.Name, parameter.Schema ?? GetDefaultSchemaForParameter(parameter));
                if (parameter.IsRequired)
                {
                    required.Add(parameter.Name);
                }
            }

            resultParameters = new Dictionary<string, object?>
            {
                { "type", "object" },
                { "required", required },
                { "properties", properties },
            };
        }
        else
        {
            resultParameters = new Dictionary<string, object?>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object?>() },
                { "required", new List<string>() },
            };
        }

        return new SparkTool.FunctionDeclaration
        {
            Name = this.FullyQualifiedName,
            Description = this.Description ?? throw new InvalidOperationException(
                $"Function description is required. Please provide a description for the function {this.FullyQualifiedName}."),
            Parameters = JsonSerializer.SerializeToNode(resultParameters),
        };
    }

    /// <summary>Gets a <see cref="KernelJsonSchema"/> for a typeless parameter with the specified description, defaulting to typeof(string)</summary>
    private static KernelJsonSchema GetDefaultSchemaForParameter(SparkFunctionParameter parameter)
    {
        // If there's a description, incorporate it.
        if (!string.IsNullOrWhiteSpace(parameter.Description))
        {
            return KernelJsonSchemaBuilder.Build(null, typeof(string), parameter.Description);
        }

        // Otherwise, we can use a cached schema for a string with no description.
        return s_stringNoDescriptionSchema;
    }
}
