namespace SuperMetroid.Core.Audio;

/// <summary>The mutually exclusive kinds of <see cref="AudioEvent"/> a frame carries to an <see cref="ClipAudioBackend"/>.</summary>
public enum AudioEventKind : byte
{
    /// <summary>Installs one audio bank before the frame's driver ticks run.</summary>
    LoadAudioBank,

    /// <summary>Latches one music command for the sequencer before the frame's driver ticks run.</summary>
    WriteMusicPort,

    /// <summary>One driver timer tick at its DSP-cycle offset within the frame.</summary>
    DriverTick,

    /// <summary>A sound effect the driver starts at the preceding tick's sound-effect poll.</summary>
    StartSoundEffect,
}
