// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.SparkDesk.Core;

internal sealed class SparkUsage
{
    /// <summary>
    /// Reserved field, can be ignored.
    /// </summary>
    [JsonPropertyName("question_tokens")]
    public int QuestionTokens { get; set; }

    /// <summary>
    /// The total tokens size of the including history questions.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// The tokens size of the answer.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// The sum of prompt_tokens and completion_tokens, also the tokens size of billing interaction in this session.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
