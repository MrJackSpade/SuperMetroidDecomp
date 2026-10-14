namespace SuperMetroid.Core.Audio;

/// <summary>Stable JSON operation names for the resident SPC sound-effect language.</summary>
public static class AudioSoundInstructionOperations
{
    /// <summary>JSON operation for native $F9: four byte arguments hold ADSR1, ADSR2, and two preserved reserved bytes; the envelope override is applied when the next note is programmed.</summary>
    public const string SetAdsr = "setAdsr";
    /// <summary>JSON operation for native $F5: subnote-delta and target-note byte arguments enable sliding with legato continuity before the following note packet.</summary>
    public const string PitchSlideLegato = "pitchSlideLegato";
    /// <summary>JSON operation for native $F8: subnote-delta and target-note byte arguments enable the non-legato pitch-slide form before the following note packet.</summary>
    public const string PitchSlide = "pitchSlide";
    /// <summary>Argument-free native $FF channel terminator, releasing the borrowed DSP voice and restoring its music-channel state; no serialized instructions may follow it.</summary>
    public const string End = "end";
    /// <summary>JSON operation for native $FE: one byte argument sets the repeat counter and records the following instruction as the repeat point.</summary>
    public const string BeginRepeat = "beginRepeat";
    /// <summary>Argument-free native $FB jump to the current repeat point without decrementing the counter; terminal in the serialized program even though playback continues looping.</summary>
    public const string RepeatForever = "repeatForever";
    /// <summary>Argument-free native $FD counted-loop terminator: decrement the byte repeat counter and return to its repeat point unless the counter reaches zero.</summary>
    public const string EndRepeat = "endRepeat";
    /// <summary>Argument-free native $FC instruction enabling DSP noise for the sound channel's borrowed voice before continuing to the next instruction.</summary>
    public const string EnableNoise = "enableNoise";
    /// <summary>Five-byte note packet ordered as instrument, volume, pan, note, and duration; the instrument itself occupies the opcode position, reserved instruction values are rejected, and note $F6 retains the previous note.</summary>
    public const string PlayNote = "playNote";
}
