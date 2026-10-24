namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect rendered dry from the driver's power-on state in one data set (#1305).
/// Frame zero is the poll that accepted the request.
/// </summary>
/// <param name="Context">The music data set loaded, or null for only the shared sounds.</param>
/// <param name="Library">The library whose port carries the request.</param>
/// <param name="Command">The library-relative request number, 1 through 255.</param>
/// <param name="Policy">How the driver accepts and starts the request.</param>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets; null for a never-ending effect.</param>
/// <param name="Layers">One layer for an audible one-shot effect, none for a silent one, or one looping layer per channel.</param>
public sealed record SoundEffectClipEntry(
    MusicDataSet? Context,
    SoundEffectLibrary Library,
    int Command,
    SoundEffectClipPolicy Policy,
    long? ActiveFrames,
    IReadOnlyList<SoundEffectClipLayer> Layers)
{
    /// <summary>The request number, validated to the range a sound-effect library port carries.</summary>
    public int Command { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);
}
