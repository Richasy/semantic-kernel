// Copyright (c) Microsoft. All rights reserved.

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Microsoft.SemanticKernel.Connectors.SparkDesk;

/// <summary>
/// Represents the version of the Spark Desk AI API.
/// </summary>
public enum SparkDeskAIVersion
{
    /// <summary>
    /// Represents the V1.5 version of the Spark Desk AI API.
    /// </summary>
    V1_5,

    /// <summary>
    /// Represents the V2 version of the Spark Desk AI API.
    /// </summary>
    V2,

    /// <summary>
    /// Represents the V3 version of the Spark Desk AI API.
    /// </summary>
    V3,

    /// <summary>
    /// Represents the V3.5 version of the Spark Desk AI API.
    /// </summary>
    V3_5,
}
