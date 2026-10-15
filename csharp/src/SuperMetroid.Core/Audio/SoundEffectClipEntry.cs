namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect rendered dry from the driver's power-on state in one bank context (#1305).
/// Frame zero is the poll that accepted the request.
/// </summary>
/// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
/// <param name="Library">The library whose port carries the request.</param>
/// <param name="Command">The request byte.</param>
/// <param name="Policy">How the driver accepts and starts the request.</param>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets; null for a never-ending effect.</param>
/// <param name="Layers">One layer for an audible one-shot effect, none for a silent one, or one looping layer per channel.</param>
public sealed record SoundEffectClipEntry(
    AudioBank? Context,
    SoundEffectLibrary Library,
    byte Command,
    SoundEffectClipPolicy Policy,
    long? ActiveFrames,
    IReadOnlyList<SoundEffectClipLayer> Layers);
