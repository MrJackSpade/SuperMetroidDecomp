namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The effects of Super Metroid's SPC music-pattern bytecode; <see cref="SpcMusicDefinitionCodec"/> owns their opcode bytes.</summary>
internal enum SpcMusicEffect
{
    /// <summary>Selects the channel's instrument sample and envelope by driver instrument index.</summary>
    SetInstrument,
    /// <summary>Sets the channel's five-bit pan position and its encoded pan flags immediately.</summary>
    SetPan,
    /// <summary>Moves channel pan toward a following target over the supplied number of ticks.</summary>
    FadePan,
    /// <summary>Starts vibrato after a delay using the following rate and depth bytes.</summary>
    EnableVibrato,
    /// <summary>Clears active and target vibrato depth and cancels its fade.</summary>
    DisableVibrato,
    /// <summary>Replaces the shared music master volume immediately.</summary>
    SetMasterVolume,
    /// <summary>Fades shared master volume toward a following target byte over the supplied ticks.</summary>
    FadeMasterVolume,
    /// <summary>Replaces the shared music tempo immediately.</summary>
    SetTempo,
    /// <summary>Fades shared tempo toward a following target byte over the supplied ticks.</summary>
    FadeTempo,
    /// <summary>Offsets pitches for every channel in the current music sequence.</summary>
    SetGlobalTransposition,
    /// <summary>Offsets pitches for only the channel reading this pattern.</summary>
    SetChannelTransposition,
    /// <summary>Starts tremolo after a delay using the following rate and depth bytes.</summary>
    EnableTremolo,
    /// <summary>Stops volume modulation by clearing the channel's tremolo depth.</summary>
    DisableTremolo,
    /// <summary>Replaces this channel's volume immediately.</summary>
    SetChannelVolume,
    /// <summary>Fades this channel's volume toward a following target byte over the supplied ticks.</summary>
    FadeChannelVolume,
    /// <summary>Jumps to a pattern pointer for the following repeat count, saving the return cursor.</summary>
    CallPattern,
    /// <summary>Reduces the current vibrato depth over the supplied number of ticks.</summary>
    FadeVibrato,
    /// <summary>Slides each note's pitch toward its envelope target after a delay.</summary>
    PitchEnvelopeTo,
    /// <summary>Slides each note's pitch from its envelope offset after a delay.</summary>
    PitchEnvelopeFrom,
    /// <summary>Stops the channel's pitch-envelope progression.</summary>
    DisablePitchEnvelope,
    /// <summary>Sets the channel's fine pitch adjustment without transposing note identities.</summary>
    SetFineTune,
    /// <summary>Enables echo writes and sets the initial left and right echo levels.</summary>
    EnableEcho,
    /// <summary>Mutes both echo levels and disables writes to the echo buffer.</summary>
    DisableEcho,
    /// <summary>Sets echo delay, feedback, and eight FIR coefficients from the uploaded driver byte space.</summary>
    ConfigureEcho,
    /// <summary>Fades left and right echo levels toward separate targets over the supplied ticks.</summary>
    FadeEchoVolume,
    /// <summary>Schedules a delayed note slide using the following duration and transposed target pitch.</summary>
    PitchSlide,
    /// <summary>Changes the base instrument identity used for subsequent percussion notes.</summary>
    SetPercussionBase,
    /// <summary>Advances past an additional pattern byte without changing playback state.</summary>
    SkipByte,
    /// <summary>Schedules a channel key cut after the encoded delay.</summary>
    CutKey,
    /// <summary>Fast-forwards for the encoded frame count plus one and keys off inactive channels.</summary>
    FastForwardForFrames,
    /// <summary>Sets the fast-forward count directly and keys off inactive channels.</summary>
    SetFastForward,
}
