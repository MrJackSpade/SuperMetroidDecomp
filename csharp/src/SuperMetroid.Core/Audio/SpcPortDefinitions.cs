namespace SuperMetroid.Core.Audio;

/// <summary>
/// The SPC driver's timing and port values that the runtime audio needs without the synthesizer
/// (#1305): the port model's driver timer and sound-effect polls, and the clip backend's music
/// latch, tempo timer and library-3 mode. The offline renderer's driver definitions refer to these.
/// </summary>
internal static class SpcPortDefinitions
{
    /// <summary>Native S-DSP stereo frames (and DSP cycles) in one 60-Hz update: 32,040 Hz / 60.</summary>
    internal const int NativeStereoFramesPerVideoFrame = 534;

    /// <summary>DSP cycles per driver timer tick (the SPC timer runs at 500 Hz).</summary>
    internal const int DspCyclesPerDriverTick = 64;

    /// <summary>Added to the sound-effect timer accumulator per driver tick; its carry is a poll, every eighth tick.</summary>
    internal const byte SoundEffectTimerStep = 0x20; // SPC timer scale, not a sequence ID

    /// <summary>The library-1 port value the driver publishes while it waits for an upload.</summary>
    internal const byte UploadReadyLibraryOnePort = 0xbb;

    /// <summary>The music latch's empty value: no command is pending.</summary>
    internal const byte MusicLatchEmpty = byte.MaxValue;

    /// <summary>The driver's power-on tempo, in force until a track sets its own.</summary>
    internal const byte InitialTempo = 0x10;

    /// <summary>Library 3's low-health mode value, in which it rejects every request except the clearing one.</summary>
    internal const byte LowHealthMode = 2;
}
