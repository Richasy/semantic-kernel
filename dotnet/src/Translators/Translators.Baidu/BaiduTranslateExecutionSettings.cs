// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Translators.Baidu;

/// <summary>
/// Baidu 翻译执行设置.
/// </summary>
public sealed class BaiduTranslateExecutionSettings : TranslateExecutionSettings
{
    private string? _from;
    private string? _to;

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
    /// 将 <see cref="TranslateExecutionSettings"/> 转换为 <see cref="BaiduTranslateExecutionSettings"/>.
    /// </summary>
    /// <param name="settings"><see cref="TranslateExecutionSettings"/> 实例.</param>
    /// <returns><see cref="BaiduTranslateExecutionSettings"/>.</returns>
    public static BaiduTranslateExecutionSettings FromExecutionSettings(TranslateExecutionSettings settings)
    {
        switch (settings)
        {
            case null:
                return new BaiduTranslateExecutionSettings();
            case BaiduTranslateExecutionSettings baiduSettings:
                return baiduSettings;
            default:
                break;
        }

        var json = JsonSerializer.Serialize(settings);
        return JsonSerializer.Deserialize<BaiduTranslateExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }

    /// <inheritdoc/>
    public override TranslateExecutionSettings Clone()
    {
        return new BaiduTranslateExecutionSettings
        {
            To = this.To,
            From = this.From,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }
}
