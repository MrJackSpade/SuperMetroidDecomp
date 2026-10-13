namespace SuperMetroid.Core.Audio;

/// <summary>Opcodes in Super Metroid's SPC music-pattern bytecode.</summary>
internal enum SpcMusicEffect : byte
{
    /// <summary>Selects the channel's instrument sample and envelope by driver instrument index.</summary>
    SetInstrument = 0xe0,
    /// <summary>Sets the channel's five-bit pan position and its encoded pan flags immediately.</summary>
    SetPan = 0xe1,
    /// <summary>Moves channel pan toward a following target over the supplied number of ticks.</summary>
    FadePan = 0xe2,
    /// <summary>Starts vibrato after a delay using the following rate and depth bytes.</summary>
    EnableVibrato = 0xe3,
    /// <summary>Clears active and target vibrato depth and cancels its fade.</summary>
    DisableVibrato = 0xe4,
    /// <summary>Replaces the shared music master volume immediately.</summary>
    SetMasterVolume = 0xe5,
    /// <summary>Fades shared master volume toward a following target byte over the supplied ticks.</summary>
    FadeMasterVolume = 0xe6,
    /// <summary>Replaces the shared music tempo immediately.</summary>
    SetTempo = 0xe7,
    /// <summary>Fades shared tempo toward a following target byte over the supplied ticks.</summary>
    FadeTempo = 0xe8,
    /// <summary>Offsets pitches for every channel in the current music sequence.</summary>
    SetGlobalTransposition = 0xe9,
    /// <summary>Offsets pitches for only the channel reading this pattern.</summary>
    SetChannelTransposition = 0xea,
    /// <summary>Starts tremolo after a delay using the following rate and depth bytes.</summary>
    EnableTremolo = 0xeb,
    /// <summary>Stops volume modulation by clearing the channel's tremolo depth.</summary>
    DisableTremolo = 0xec,
    /// <summary>Replaces this channel's volume immediately.</summary>
    SetChannelVolume = 0xed,
    /// <summary>Fades this channel's volume toward a following target byte over the supplied ticks.</summary>
    FadeChannelVolume = 0xee,
    /// <summary>Jumps to a pattern pointer for the following repeat count, saving the return cursor.</summary>
    CallPattern = 0xef,
    /// <summary>Reduces the current vibrato depth over the supplied number of ticks.</summary>
    FadeVibrato = 0xf0,
    /// <summary>Slides each note's pitch toward its envelope target after a delay.</summary>
    PitchEnvelopeTo = 0xf1,
    /// <summary>Slides each note's pitch from its envelope offset after a delay.</summary>
    PitchEnvelopeFrom = 0xf2,
    /// <summary>Stops the channel's pitch-envelope progression.</summary>
    DisablePitchEnvelope = 0xf3,
    /// <summary>Sets the channel's fine pitch adjustment without transposing note identities.</summary>
    SetFineTune = 0xf4,
    /// <summary>Enables echo writes and sets the initial left and right echo levels.</summary>
    EnableEcho = 0xf5,
    /// <summary>Mutes both echo levels and disables writes to the echo buffer.</summary>
    DisableEcho = 0xf6,
    /// <summary>Sets echo delay, feedback, and eight FIR coefficients from the uploaded driver byte space.</summary>
    ConfigureEcho = 0xf7,
    /// <summary>Fades left and right echo levels toward separate targets over the supplied ticks.</summary>
    FadeEchoVolume = 0xf8,
    /// <summary>Schedules a delayed note slide using the following duration and transposed target pitch.</summary>
    PitchSlide = 0xf9,
    /// <summary>Changes the base instrument identity used for subsequent percussion notes.</summary>
    SetPercussionBase = 0xfa,
    /// <summary>Advances past an additional pattern byte without changing playback state.</summary>
    SkipByte = 0xfb,
    /// <summary>Schedules a channel key cut after the encoded delay.</summary>
    CutKey = 0xfc,
    /// <summary>Fast-forwards for the encoded frame count plus one and keys off inactive channels.</summary>
    FastForwardForFrames = 0xfd,
    /// <summary>Sets the fast-forward count directly and keys off inactive channels.</summary>
    SetFastForward = 0xfe,
}
