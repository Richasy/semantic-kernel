// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticKernel.Translators.Ali.Core;

namespace Microsoft.SemanticKernel.Translators.Ali;

/// <summary>
/// Ali 翻译执行设置.
/// </summary>
public sealed class AliTranslateExecutionSettings : TranslateExecutionSettings
{
    private string? _from;
    private string? _to;
    private FormatType _formatType = FormatType.Text;

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
    [JsonPropertyName("formatType")]
    public FormatType FormatType
    {
        get => this._formatType;
        set
        {
            this.ThrowIfFrozen();
            this._formatType = value;
        }
    }

    /// <summary>
    /// 将 <see cref="TranslateExecutionSettings"/> 转换为 <see cref="AliTranslateExecutionSettings"/>.
    /// </summary>
    /// <param name="settings"><see cref="TranslateExecutionSettings"/> 实例.</param>
    /// <returns><see cref="AliTranslateExecutionSettings"/>.</returns>
    public static AliTranslateExecutionSettings FromExecutionSettings(TranslateExecutionSettings settings)
    {
        switch (settings)
        {
            case null:
                return new AliTranslateExecutionSettings();
            case AliTranslateExecutionSettings aliSettings:
                return aliSettings;
            default:
                break;
        }

        var json = JsonSerializer.Serialize(settings);
        return JsonSerializer.Deserialize<AliTranslateExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }

    /// <inheritdoc/>
    public override TranslateExecutionSettings Clone()
    {
        return new AliTranslateExecutionSettings
        {
            To = this.To,
            From = this.From,
            FormatType = this.FormatType,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }
}
