namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The S-DSP's named global registers; <see cref="SnesDspRegisterLayout"/> owns their register-file addresses.</summary>
internal enum DspGlobalRegister
{
    /// <summary>MVOLL: left master volume.</summary>
    MasterVolumeLeft,
    /// <summary>MVOLR: right master volume.</summary>
    MasterVolumeRight,
    /// <summary>EVOLL: left echo volume.</summary>
    EchoVolumeLeft,
    /// <summary>EVOLR: right echo volume.</summary>
    EchoVolumeRight,
    /// <summary>KON: key-on voice bits.</summary>
    KeyOn,
    /// <summary>KOFF: key-off voice bits.</summary>
    KeyOff,
    /// <summary>FLG: reset, mute, echo-write and noise-rate flags.</summary>
    Flags,
    /// <summary>ENDX: voice end flags, published by the DSP.</summary>
    EndFlags,
    /// <summary>EFB: echo feedback.</summary>
    EchoFeedback,
    /// <summary>PMON: pitch-modulation voice bits.</summary>
    PitchModulation,
    /// <summary>NON: noise voice bits.</summary>
    NoiseEnable,
    /// <summary>EON: echo voice bits.</summary>
    EchoEnable,
    /// <summary>DIR: BRR sample directory page.</summary>
    SourceDirectory,
    /// <summary>ESA: echo buffer page.</summary>
    EchoBufferAddress,
    /// <summary>EDL: echo delay.</summary>
    EchoDelay,
}
