// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticKernel.Translators.Azure.Models;

namespace Microsoft.SemanticKernel.Translators.Azure;

/// <summary>
/// Azure 翻译执行设置.
/// </summary>
public sealed class AzureTranslateExecutionSettings : TranslateExecutionSettings
{
    private string? _from;
    private string? _to;
    private TextType _textType = TextType.Plain;

    /// <summary>
    /// 输出文本的语言.
    /// </summary>
    [JsonPropertyName("to")]
    public string? To
    {
        get => this._to;
        set
        {
            this.ThrowIfFrozen();
            this._to = value;
        }
    }

    /// <summary>
    /// 输入文本的语言.
    /// </summary>
    [JsonPropertyName("from")]
    public string? From
    {
        get => this._from;
        set
        {
            this.ThrowIfFrozen();
            this._from = value;
        }
    }

    /// <summary>
    /// 文本类型.
    /// </summary>
    [JsonPropertyName("textType")]
    public TextType TextType
    {
        get => this._textType;
        set
        {
            this.ThrowIfFrozen();
            this._textType = value;
        }
    }

    /// <summary>
    /// 将 <see cref="TranslateExecutionSettings"/> 转换为 <see cref="AzureTranslateExecutionSettings"/>.
    /// </summary>
    /// <param name="settings"><see cref="TranslateExecutionSettings"/> 实例.</param>
    /// <returns><see cref="AzureTranslateExecutionSettings"/>.</returns>
    public static AzureTranslateExecutionSettings FromExecutionSettings(TranslateExecutionSettings settings)
    {
        switch (settings)
        {
            case null:
                return new AzureTranslateExecutionSettings();
            case AzureTranslateExecutionSettings azureSettings:
                return azureSettings;
            default:
                break;
        }

        var json = JsonSerializer.Serialize(settings);
        return JsonSerializer.Deserialize<AzureTranslateExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }

    /// <inheritdoc/>
    public override TranslateExecutionSettings Clone()
    {
        return new AzureTranslateExecutionSettings
        {
            To = this.To,
            From = this.From,
            TextType = this.TextType,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }
}
