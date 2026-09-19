namespace SuperMetroid.Core.Audio;

/// <summary>Resident SPC sound-effect bytecode identifiers used by authored channel programs.</summary>
internal static class SpcSoundEffectOpcodes
{
    /// <summary>`$F5`: enable a legato pitch slide before the following note packet.</summary>
    internal const byte PitchSlideLegato = 0xf5;
    /// <summary>`$F8`: enable a retriggered pitch slide before the following note packet.</summary>
    internal const byte PitchSlide = 0xf8;
    /// <summary>`$F9`: replace ADSR1/ADSR2; two following bytes retain native reserved data.</summary>
    internal const byte SetAdsr = 0xf9;
    /// <summary>`$FB`: return to the current repeat point forever.</summary>
    internal const byte RepeatForever = 0xfb;
    /// <summary>`$FC`: enable DSP noise for the borrowed voice.</summary>
    internal const byte EnableNoise = 0xfc;
    /// <summary>`$FD`: counted repeat terminator.</summary>
    internal const byte EndRepeat = 0xfd;
    /// <summary>`$FE`: establish a repeat count and repeat point.</summary>
    internal const byte BeginRepeat = 0xfe;
    /// <summary>`$FF`: end the channel and release its borrowed voice.</summary>
    internal const byte End = byte.MaxValue;
}
