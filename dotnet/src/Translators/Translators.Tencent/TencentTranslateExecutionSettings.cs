// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Text;

namespace Microsoft.SemanticKernel.Translators.Tencent;

/// <summary>
/// Tencent 翻译执行设置.
/// </summary>
public sealed class TencentTranslateExecutionSettings : TranslateExecutionSettings
{
    private string? _from;
    private string? _to;
    private string? _untranslatedText;

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
    /// 不需要翻译的文本.
    /// </summary>
    [JsonPropertyName("untranslatedText")]
    public string? UntranslatedText
    {
        get => this._untranslatedText;
        set
        {
            this.ThrowIfFrozen();
            this._untranslatedText = value;
        }
    }

    /// <summary>
    /// 将 <see cref="TranslateExecutionSettings"/> 转换为 <see cref="TencentTranslateExecutionSettings"/>.
    /// </summary>
    /// <param name="settings"><see cref="TranslateExecutionSettings"/> 实例.</param>
    /// <returns><see cref="TencentTranslateExecutionSettings"/>.</returns>
    public static TencentTranslateExecutionSettings FromExecutionSettings(TranslateExecutionSettings settings)
    {
        switch (settings)
        {
            case null:
                return new TencentTranslateExecutionSettings();
            case TencentTranslateExecutionSettings tencentSettings:
                return tencentSettings;
        }

        var json = JsonSerializer.Serialize(settings);
        return JsonSerializer.Deserialize<TencentTranslateExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }

    /// <inheritdoc/>
    public override TranslateExecutionSettings Clone()
    {
        return new TencentTranslateExecutionSettings
        {
            To = this.To,
            From = this.From,
            UntranslatedText = this.UntranslatedText,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }
}
