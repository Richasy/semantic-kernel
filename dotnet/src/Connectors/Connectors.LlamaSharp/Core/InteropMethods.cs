// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;

internal static class InteropMethods
{
    public static global::LLama.Common.ChatHistory ToLLamaSharpChatHistory(this ChatHistory chatHistory, bool ignoreCase = true)
    {
        if (chatHistory is null)
        {
            throw new ArgumentNullException(nameof(chatHistory));
        }

        var history = new global::LLama.Common.ChatHistory();

        foreach (var chat in chatHistory)
        {
            var role = Enum.TryParse<global::LLama.Common.AuthorRole>(chat.Role.Label, ignoreCase, out var _role) ? _role : global::LLama.Common.AuthorRole.Unknown;
            history.AddMessage(role, chat.Content ?? string.Empty);
        }

        return history;
    }

    /// <summary>
    /// Convert LLamaSharpPromptExecutionSettings to LLamaSharp InferenceParams
    /// </summary>
    /// <param name="requestSettings"></param>
    /// <returns></returns>
    internal static global::LLama.Common.InferenceParams ToLLamaSharpInferenceParams(this LlamaSharpPromptExecutionSettings requestSettings)
    {
        if (requestSettings is null)
        {
            throw new ArgumentNullException(nameof(requestSettings));
        }

        var antiPrompts = new List<string>(requestSettings.StopSequences ?? [])
                                  { LLama.Common.AuthorRole.User.ToString() + ":" ,
                                    LLama.Common.AuthorRole.Assistant.ToString() + ":",
                                    LLama.Common.AuthorRole.System.ToString() + ":"};
        return new global::LLama.Common.InferenceParams
        {
            Temperature = (float)requestSettings.Temperature,
            TopP = (float)requestSettings.TopP,
            PresencePenalty = (float)requestSettings.PresencePenalty,
            FrequencyPenalty = (float)requestSettings.FrequencyPenalty,
            AntiPrompts = antiPrompts,
            MaxTokens = requestSettings.MaxTokens ?? -1
        };
    }
}
