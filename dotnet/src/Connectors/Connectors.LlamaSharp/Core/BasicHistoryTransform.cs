// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using LLama.Abstractions;
using LLama.Common;

namespace Microsoft.SemanticKernel.Connectors.LlamaSharp.Core;

internal sealed class BasicHistoryTransform : IHistoryTransform
{
    private readonly string _systemTemplate;
    private readonly string _userTemplate;
    private readonly string _assistantTemplate;

    public BasicHistoryTransform(string? systemTemplate = default, string? userTemplate = default, string? assistantTemplate = default)
    {
        this._systemTemplate = systemTemplate ?? "System: {{system}}\n";
        this._userTemplate = userTemplate ?? "User: {{user}}\n";
        this._assistantTemplate = assistantTemplate ?? "Assistant: {{assistant}}\n";

        var keywords = new List<string>();
        AddKeywords(this._systemTemplate, "{{system}}");
        AddKeywords(this._userTemplate, "{{user}}");
        AddKeywords(this._assistantTemplate, "{{assistant}}");
        this.Keywords = keywords;

        void AddKeywords(string template, string splitKey)
        {
            if (template.Contains(splitKey))
            {
                var sp = template.Split([splitKey], System.StringSplitOptions.RemoveEmptyEntries);
                if (sp.Length > 0)
                {
                    foreach (var item in sp)
                    {
                        var keyword = item.Trim();
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            keywords.Add(keyword);
                        }
                    }
                }
            }

            keywords = keywords.Distinct().ToList();
        }
    }

    public IEnumerable<string> Keywords { get; private set; }

    public IHistoryTransform Clone()
    {
        return new BasicHistoryTransform(this._systemTemplate, this._userTemplate, this._assistantTemplate);
    }

    public string HistoryToText(ChatHistory history)
    {
        var historyText = string.Join("\n", history.Messages.Select(this.GetMessageText));
        var assistantPrefix = this._assistantTemplate.Split(["{{assistant}}"], System.StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        return $"{historyText}\n{assistantPrefix}";
    }

    public ChatHistory TextToHistory(AuthorRole role, string text)
    {
        var history = new ChatHistory();
        history.AddMessage(role, this.TrimNamesFromText(text));
        return history;
    }
    private string TrimNamesFromText(string text)
    {
        if (this.Keywords?.Count() > 0)
        {
            foreach (var keyword in this.Keywords)
            {
                text = text.Replace(keyword, string.Empty);
            }
        }

        return text;
    }

    private string GetMessageText(ChatHistory.Message message)
    {
        return message.AuthorRole switch
        {
            AuthorRole.System => this._systemTemplate.Replace("{{system}}", message.Content),
            AuthorRole.User => this._userTemplate.Replace("{{user}}", message.Content),
            AuthorRole.Assistant => this._assistantTemplate.Replace("{{assistant}}", message.Content),
            _ => "unknown",
        };
    }
}
