namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect rendered dry (#1305): it sounds the same whichever music data set is loaded (#2057).
/// Frame zero is the request check that accepted the request.
/// </summary>
/// <param name="Sound">The sound effect, whose library plays it.</param>
/// <param name="Policy">How the library accepts and starts the request.</param>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets; null for a never-ending effect.</param>
/// <param name="Layers">One layer for an audible one-shot effect, none for a silent one, or one looping layer per channel.</param>
public sealed record SoundEffectClipEntry(
    SoundEffectId Sound,
    SoundEffectClipPolicy Policy,
    long? ActiveFrames,
    IReadOnlyList<SoundEffectClipLayer> Layers);
