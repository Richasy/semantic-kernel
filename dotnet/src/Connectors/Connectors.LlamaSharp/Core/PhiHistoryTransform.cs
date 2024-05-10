// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using LLama.Abstractions;
using LLama.Common;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;

internal sealed class PhiHistoryTransform : IHistoryTransform
{
    private const string SYSTEM_PREFIX = "<|system|>";
    private const string USER_PREFIX = "<|user|>";
    private const string ASSISTANT_PREFIX = "<|assistant|>";
    private const string END_SUFFIX = "<|end|>";

    public static IEnumerable<string> Keywrods => [USER_PREFIX, ASSISTANT_PREFIX, SYSTEM_PREFIX, END_SUFFIX];

    public IHistoryTransform Clone()
    {
        return new PhiHistoryTransform();
    }

    public string HistoryToText(ChatHistory history)
    {
        var historyText = string.Join("\n", history.Messages.Select(this.GetMessageText));
        return $"{historyText}\n{this.GetPrefix(AuthorRole.Assistant)}";
    }

    public ChatHistory TextToHistory(AuthorRole role, string text)
    {
        var history = new ChatHistory();
        history.AddMessage(role, this.TrimNamesFromText(text, role));
        return history;
    }
    private string TrimNamesFromText(string text, AuthorRole role)
    {
        if (role == AuthorRole.User && text.StartsWith(USER_PREFIX + "\n", System.StringComparison.InvariantCultureIgnoreCase))
        {
            text = text.Substring((USER_PREFIX).Length).Replace(END_SUFFIX, string.Empty).Trim();
        }
        else if (role == AuthorRole.Assistant && text.EndsWith(ASSISTANT_PREFIX, System.StringComparison.InvariantCultureIgnoreCase))
        {
            text = text.Substring(0, text.Length - ASSISTANT_PREFIX.Length).TrimEnd();
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
            _ => "<|unknown|>",
        };
    }

    private string GetMessageText(ChatHistory.Message message)
    {
        var roleText = this.GetPrefix(message.AuthorRole);
        return $"{roleText}\n{message.Content}{END_SUFFIX}";
    }
}
