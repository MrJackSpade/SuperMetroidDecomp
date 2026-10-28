namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The S-DSP's named global registers, valued by their register-file address.</summary>
internal enum DspGlobalRegister
{
    /// <summary>MVOLL: left master volume.</summary>
    MasterVolumeLeft = 0x0c,
    /// <summary>MVOLR: right master volume.</summary>
    MasterVolumeRight = 0x1c,
    /// <summary>EVOLL: left echo volume.</summary>
    EchoVolumeLeft = 0x2c,
    /// <summary>EVOLR: right echo volume.</summary>
    EchoVolumeRight = 0x3c,
    /// <summary>KON: key-on voice bits.</summary>
    KeyOn = 0x4c,
    /// <summary>KOFF: key-off voice bits.</summary>
    KeyOff = 0x5c,
    /// <summary>FLG: reset, mute, echo-write and noise-rate flags.</summary>
    Flags = 0x6c,
    /// <summary>ENDX: voice end flags, published by the DSP.</summary>
    EndFlags = 0x7c,
    /// <summary>EFB: echo feedback.</summary>
    EchoFeedback = 0x0d,
    /// <summary>PMON: pitch-modulation voice bits.</summary>
    PitchModulation = 0x2d,
    /// <summary>NON: noise voice bits.</summary>
    NoiseEnable = 0x3d,
    /// <summary>EON: echo voice bits.</summary>
    EchoEnable = 0x4d,
    /// <summary>DIR: BRR sample directory page.</summary>
    SourceDirectory = 0x5d,
    /// <summary>ESA: echo buffer page.</summary>
    EchoBufferAddress = 0x6d,
    /// <summary>EDL: echo delay.</summary>
    EchoDelay = 0x7d,
}
