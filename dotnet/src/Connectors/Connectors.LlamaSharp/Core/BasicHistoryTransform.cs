// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using LLama.Abstractions;
using LLama.Common;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;

internal sealed class BasicHistoryTransform : IHistoryTransform
{
    private const string START_PREFIX = "<|im_start|>";
    private const string END_SUFFIX = "<|im_end|>";
    private const string SYSTEM_PREFIX = "system";
    private const string USER_PREFIX = "user";
    private const string ASSISTANT_PREFIX = "assistant";

    public static IEnumerable<string> Keywrods => [USER_PREFIX, ASSISTANT_PREFIX, SYSTEM_PREFIX, START_PREFIX, END_SUFFIX];

    public IHistoryTransform Clone()
    {
        return new PhiHistoryTransform();
    }

    public string HistoryToText(ChatHistory history)
    {
        var historyText = string.Join("\n", history.Messages.Select(this.GetMessageText));
        return $"{historyText}\n{START_PREFIX}{this.GetPrefix(AuthorRole.Assistant)}";
    }

    public ChatHistory TextToHistory(AuthorRole role, string text)
    {
        var history = new ChatHistory();
        history.AddMessage(role, this.TrimNamesFromText(text, role));
        return history;
    }
    private string TrimNamesFromText(string text, AuthorRole role)
    {
        if (role == AuthorRole.User && text.StartsWith(START_PREFIX + USER_PREFIX, System.StringComparison.InvariantCultureIgnoreCase))
        {
            text = text.Substring(START_PREFIX.Length + USER_PREFIX.Length).Replace(END_SUFFIX, string.Empty).Trim();
        }
        else if (role == AuthorRole.Assistant && text.EndsWith(START_PREFIX + ASSISTANT_PREFIX, System.StringComparison.InvariantCultureIgnoreCase))
        {
            text = text.Substring(0, text.Length - START_PREFIX.Length - ASSISTANT_PREFIX.Length).TrimEnd();
        }

        return text;
    }


    private string GetPrefix(AuthorRole role)
    {
        return role switch
        {
            AuthorRole.System => SYSTEM_PREFIX,
            AuthorRole.User => USER_PREFIX,
            AuthorRole.Assistant => ASSISTANT_PREFIX,
            _ => "unknown",
        };
    }

    private string GetMessageText(ChatHistory.Message message)
    {
        var roleText = this.GetPrefix(message.AuthorRole);
        return $"{START_PREFIX}{roleText}\n{message.Content}{END_SUFFIX}";
    }
}
