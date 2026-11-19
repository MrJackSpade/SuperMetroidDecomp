using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// One operation of the resident SPC sound-effect language (#2548): decoded once from a channel program's bytes and
/// written into the installed audio manifest by its key in <see cref="AudioSoundOperationKeys"/>.
/// </summary>
[JsonConverter(typeof(AudioSoundOperationJsonConverter))]
public enum AudioSoundOperation
{
    /// <summary>Native $F9: four byte arguments hold ADSR1, ADSR2 and two preserved reserved bytes; the envelope override is applied when the next note is programmed.</summary>
    SetAdsr,

    /// <summary>Native $F5: subnote-delta and target-note byte arguments enable sliding with legato continuity before the following note packet.</summary>
    PitchSlideLegato,

    /// <summary>Native $F8: subnote-delta and target-note byte arguments enable the non-legato pitch-slide form before the following note packet.</summary>
    PitchSlide,

    /// <summary>Argument-free native $FF channel terminator, releasing the borrowed DSP voice and restoring its music-channel state; no instruction may follow it.</summary>
    End,

    /// <summary>Native $FE: one byte argument sets the repeat counter and records the following instruction as the repeat point.</summary>
    BeginRepeat,

    /// <summary>Argument-free native $FB jump to the current repeat point without decrementing the counter; terminal in the program even though playback continues looping.</summary>
    RepeatForever,

    /// <summary>Argument-free native $FD counted-loop terminator: decrement the repeat counter and return to its repeat point unless it reaches zero.</summary>
    EndRepeat,

    /// <summary>Argument-free native $FC instruction enabling DSP noise for the sound channel's borrowed voice before the next instruction.</summary>
    EnableNoise,

    /// <summary>Five-byte note packet ordered as instrument, volume, pan, note and duration; the instrument occupies the opcode position, reserved instruction values are rejected, and note $F6 retains the previous note.</summary>
    PlayNote,
}
