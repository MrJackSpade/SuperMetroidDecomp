namespace SuperMetroid.Core.Audio;

/// <summary>
/// Music-port control bytes. Every other byte the CPU writes to the music port is a track
/// number, which the driver starts unless it is already playing.
/// </summary>
internal enum MusicPortCommand : byte
{
    /// <summary>Keys off every music channel without changing the track.</summary>
    Pause = AudioRomData.Apu.PauseMusic,
    /// <summary>Resumes the paused track.</summary>
    Resume = AudioRomData.Apu.ResumeMusic,
    /// <summary>The driver's empty-port value, written after it consumes a command.</summary>
    None = byte.MaxValue,
}
