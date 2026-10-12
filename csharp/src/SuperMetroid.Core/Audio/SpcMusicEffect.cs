namespace SuperMetroid.Core.Audio;

/// <summary>Opcodes in Super Metroid's SPC music-pattern bytecode.</summary>
internal enum SpcMusicEffect : byte
{
    /// <summary>Identifies set instrument.</summary>
    SetInstrument = 0xe0,
    /// <summary>Identifies set pan.</summary>
    SetPan = 0xe1,
    /// <summary>Identifies fade pan.</summary>
    FadePan = 0xe2,
    /// <summary>Identifies enable vibrato.</summary>
    EnableVibrato = 0xe3,
    /// <summary>Identifies disable vibrato.</summary>
    DisableVibrato = 0xe4,
    /// <summary>Identifies set master volume.</summary>
    SetMasterVolume = 0xe5,
    /// <summary>Identifies fade master volume.</summary>
    FadeMasterVolume = 0xe6,
    /// <summary>Identifies set tempo.</summary>
    SetTempo = 0xe7,
    /// <summary>Identifies fade tempo.</summary>
    FadeTempo = 0xe8,
    /// <summary>Identifies set global transposition.</summary>
    SetGlobalTransposition = 0xe9,
    /// <summary>Identifies set channel transposition.</summary>
    SetChannelTransposition = 0xea,
    /// <summary>Identifies enable tremolo.</summary>
    EnableTremolo = 0xeb,
    /// <summary>Identifies disable tremolo.</summary>
    DisableTremolo = 0xec,
    /// <summary>Identifies set channel volume.</summary>
    SetChannelVolume = 0xed,
    /// <summary>Identifies fade channel volume.</summary>
    FadeChannelVolume = 0xee,
    /// <summary>Identifies call pattern.</summary>
    CallPattern = 0xef,
    /// <summary>Identifies fade vibrato.</summary>
    FadeVibrato = 0xf0,
    /// <summary>Identifies pitch envelope to.</summary>
    PitchEnvelopeTo = 0xf1,
    /// <summary>Identifies pitch envelope from.</summary>
    PitchEnvelopeFrom = 0xf2,
    /// <summary>Identifies disable pitch envelope.</summary>
    DisablePitchEnvelope = 0xf3,
    /// <summary>Identifies set fine tune.</summary>
    SetFineTune = 0xf4,
    /// <summary>Identifies enable echo.</summary>
    EnableEcho = 0xf5,
    /// <summary>Identifies disable echo.</summary>
    DisableEcho = 0xf6,
    /// <summary>Identifies configure echo.</summary>
    ConfigureEcho = 0xf7,
    /// <summary>Identifies fade echo volume.</summary>
    FadeEchoVolume = 0xf8,
    /// <summary>Identifies pitch slide.</summary>
    PitchSlide = 0xf9,
    /// <summary>Identifies set percussion base.</summary>
    SetPercussionBase = 0xfa,
    /// <summary>Identifies skip byte.</summary>
    SkipByte = 0xfb,
    /// <summary>Identifies cut key.</summary>
    CutKey = 0xfc,
    /// <summary>Identifies fast forward for frames.</summary>
    FastForwardForFrames = 0xfd,
    /// <summary>Identifies set fast forward.</summary>
    SetFastForward = 0xfe,
}
