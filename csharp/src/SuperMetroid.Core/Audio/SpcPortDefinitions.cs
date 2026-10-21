namespace SuperMetroid.Core.Audio;

/// <summary>
/// The SPC driver's timing and port values that the runtime audio needs without the synthesizer
/// (#1305): the update's sample count, and the clip backend's music latch, tempo timer and
/// library-3 mode; the request-check timing is the <see cref="SoundAcceptanceClock"/>'s. The offline renderer's driver definitions refer to these.
/// </summary>
internal static class SpcPortDefinitions
{
    /// <summary>Native S-DSP stereo frames (and DSP cycles) in one 60-Hz update: 32,040 Hz / 60.</summary>
    internal const int NativeStereoFramesPerVideoFrame = SoundAcceptanceClock.SamplesPerUpdate;

    /// <summary>The music latch's empty value: no command is pending.</summary>
    internal const byte MusicLatchEmpty = byte.MaxValue;

    /// <summary>The driver's power-on tempo, in force until a track sets its own.</summary>
    internal const byte InitialTempo = 0x10;

    /// <summary>Library 3's low-health mode value, in which it rejects every request except the clearing one.</summary>
    internal const byte LowHealthMode = 2;
}
