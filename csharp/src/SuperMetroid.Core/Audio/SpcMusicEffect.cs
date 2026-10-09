namespace SuperMetroid.Core.Audio;

/// <summary>Opcodes in Super Metroid's SPC music-pattern bytecode.</summary>
internal enum SpcMusicEffect : byte
{
    /// <summary>Selects an instrument-table record for the active channel.</summary>
    SetInstrument = 0xe0,
    /// <summary>Sets the channel pan position immediately.</summary>
    SetPan = 0xe1,
    /// <summary>Fades the channel pan position over the supplied tick count.</summary>
    FadePan = 0xe2,
    /// <summary>Enables vibrato with a delay, rate, and depth.</summary>
    EnableVibrato = 0xe3,
    /// <summary>Disables vibrato and clears its current depth.</summary>
    DisableVibrato = 0xe4,
    /// <summary>Sets the global music master volume.</summary>
    SetMasterVolume = 0xe5,
    /// <summary>Fades the global master volume over the supplied tick count.</summary>
    FadeMasterVolume = 0xe6,
    /// <summary>Sets the global music tempo.</summary>
    SetTempo = 0xe7,
    /// <summary>Fades the global tempo over the supplied tick count.</summary>
    FadeTempo = 0xe8,
    /// <summary>Sets the signed transposition applied to all channels.</summary>
    SetGlobalTransposition = 0xe9,
    /// <summary>Sets the active channel's signed note transposition.</summary>
    SetChannelTransposition = 0xea,
    /// <summary>Enables channel tremolo with delay, rate, and depth operands.</summary>
    EnableTremolo = 0xeb,
    /// <summary>Disables channel tremolo and clears its depth.</summary>
    DisableTremolo = 0xec,
    /// <summary>Sets the active channel's volume immediately.</summary>
    SetChannelVolume = 0xed,
    /// <summary>Fades the active channel's volume over the supplied tick count.</summary>
    FadeChannelVolume = 0xee,
    /// <summary>Calls a nested channel pattern at the supplied APU-RAM address.</summary>
    CallPattern = 0xef,
    /// <summary>Fades vibrato depth toward its configured target.</summary>
    FadeVibrato = 0xf0,
    /// <summary>Slides pitch toward a target note using the configured envelope timing.</summary>
    PitchEnvelopeTo = 0xf1,
    /// <summary>Slides pitch away from a target note using the configured envelope timing.</summary>
    PitchEnvelopeFrom = 0xf2,
    /// <summary>Disables the active pitch envelope.</summary>
    DisablePitchEnvelope = 0xf3,
    /// <summary>Sets the active channel's fine-pitch byte.</summary>
    SetFineTune = 0xf4,
    /// <summary>Enables DSP echo routing for the music channels.</summary>
    EnableEcho = 0xf5,
    /// <summary>Disables DSP echo routing for the music channels.</summary>
    DisableEcho = 0xf6,
    /// <summary>Configures echo delay and FIR preset selection.</summary>
    ConfigureEcho = 0xf7,
    /// <summary>Fades left and right echo output volumes.</summary>
    FadeEchoVolume = 0xf8,
    /// <summary>Slides the active channel pitch toward a note with explicit delay and duration.</summary>
    PitchSlide = 0xf9,
    /// <summary>Sets the base instrument number used to decode percussion note opcodes.</summary>
    SetPercussionBase = 0xfa,
    /// <summary>Skips the following byte in the channel program.</summary>
    SkipByte = 0xfb,
    /// <summary>Sets the active channel's cut-key state.</summary>
    CutKey = 0xfc,
    /// <summary>Enables driver fast-forward for the supplied frame count.</summary>
    FastForwardForFrames = 0xfd,
    /// <summary>Sets the driver fast-forward state directly.</summary>
    SetFastForward = 0xfe,
}
