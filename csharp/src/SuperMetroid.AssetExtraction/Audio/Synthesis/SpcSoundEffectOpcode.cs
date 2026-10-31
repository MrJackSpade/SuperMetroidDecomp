namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Resident SPC sound-effect bytecode instructions used by authored channel programs; <see cref="SpcSoundEffectProgramCodec"/>
/// owns their opcode bytes.
/// </summary>
internal enum SpcSoundEffectOpcode
{
    /// <summary>Enable a legato pitch slide before the following note packet.</summary>
    PitchSlideLegato,
    /// <summary>Enable a retriggered pitch slide before the following note packet.</summary>
    PitchSlide,
    /// <summary>Replace ADSR1/ADSR2; two following bytes retain native reserved data.</summary>
    SetAdsr,
    /// <summary>Return to the current repeat point forever.</summary>
    RepeatForever,
    /// <summary>Enable DSP noise for the borrowed voice.</summary>
    EnableNoise,
    /// <summary>Counted repeat terminator.</summary>
    EndRepeat,
    /// <summary>Establish a repeat count and repeat point.</summary>
    BeginRepeat,
    /// <summary>End the channel and release its borrowed voice.</summary>
    End,
}
