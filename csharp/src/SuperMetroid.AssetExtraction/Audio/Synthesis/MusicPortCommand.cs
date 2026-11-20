namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The music port's controls. Every other byte the CPU writes to the music port is a track number, which the driver starts
/// unless it is already playing; <see cref="MusicPortCommands"/> holds each control's byte.
/// </summary>
internal enum MusicPortCommand
{
    /// <summary>Keys off every music channel without changing the track.</summary>
    Pause,
    /// <summary>Resumes the paused track.</summary>
    Resume,
    /// <summary>The driver's empty port, which it reads after consuming a command.</summary>
    None,
}
