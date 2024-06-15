// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Speakers.Azure;

/// <summary>
/// Azure 文本转音频执行设置.
/// </summary>
public sealed class AzureTextToAudioExecutionSettings : PromptExecutionSettings
{
    private string? _voice;
    private string? _gender;
    private string? _language;
    private double _speed = 1.0;

    /// <summary>
    /// The voice to use when generating the audio.
    /// </summary>
    [JsonPropertyName("voice")]
    public string Voice
    {
        get => this._voice ?? "en-US-ChristopherNeural";

        set
        {
            this.ThrowIfFrozen();
            this._voice = value;
        }
    }

    /// <summary>
    /// Voice gender.
    /// </summary>
    [JsonPropertyName("gender")]
    public string Gender
    {
        get => this._gender ?? "male";

        set
        {
            this.ThrowIfFrozen();
            this._gender = value;
        }
    }

    /// <summary>
    /// Voice language.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language
    {
        get => this._language ?? "en-US";

        set
        {
            this.ThrowIfFrozen();
            this._language = value;
        }
    }

    /// <summary>
    /// The speed of the generated audio. Select a value from 0.25 to 4.0. 1.0 is the default.
    /// </summary>
    [JsonPropertyName("speed")]
    public double Speed
    {
        get => this._speed;

        set
        {
            this.ThrowIfFrozen();
            this._speed = value;
        }
    }
}
