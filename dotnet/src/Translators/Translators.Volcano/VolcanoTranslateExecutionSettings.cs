// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Translators.Volcano;

/// <summary>
/// 火山翻译执行设置.
/// </summary>
public sealed class VolcanoTranslateExecutionSettings : TranslateExecutionSettings
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
    /// 将 <see cref="TranslateExecutionSettings"/> 转换为 <see cref="VolcanoTranslateExecutionSettings"/>.
    /// </summary>
    /// <param name="settings"><see cref="TranslateExecutionSettings"/> 实例.</param>
    /// <returns><see cref="VolcanoTranslateExecutionSettings"/>.</returns>
    public static VolcanoTranslateExecutionSettings FromExecutionSettings(TranslateExecutionSettings settings)
    {
        switch (settings)
        {
            case null:
                return new VolcanoTranslateExecutionSettings();
            case VolcanoTranslateExecutionSettings VolcanoSettings:
                return VolcanoSettings;
        }

        var json = JsonSerializer.Serialize(settings);
        return JsonSerializer.Deserialize<VolcanoTranslateExecutionSettings>(json, JsonOptionsCache.ReadPermissive)!;
    }

    /// <inheritdoc/>
    public override TranslateExecutionSettings Clone()
    {
        return new VolcanoTranslateExecutionSettings
        {
            To = this.To,
            From = this.From,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
        };
    }
}
