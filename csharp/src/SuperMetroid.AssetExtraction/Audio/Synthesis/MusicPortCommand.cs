using SuperMetroid.Core.Audio;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Music-port control bytes. Every other byte the CPU writes to the music port is a track
/// number, which the driver starts unless it is already playing.
/// </summary>
internal enum MusicPortCommand : byte
{
    /// <summary>Keys off every music channel without changing the track.</summary>
    Pause = 0xf0, // driver pause command
    /// <summary>Resumes the paused track.</summary>
    Resume = 0xf1, // driver resume command
    /// <summary>The driver's empty-port value, written after it consumes a command.</summary>
    None = SpcPortDefinitions.MusicLatchEmpty,
}
