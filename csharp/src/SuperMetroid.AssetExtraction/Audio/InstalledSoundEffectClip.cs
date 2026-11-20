using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One sound effect rendered dry once, as the clip manifest stores it (#1305, #2057): the entry the game plays, whichever
/// music data set is loaded. Frame zero is the request check that accepted the request.
/// </summary>
/// <param name="Sound">The sound effect, whose library plays it.</param>
/// <param name="Policy">How the library accepts and starts the request.</param>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets; null for a never-ending effect.</param>
/// <param name="Layers">One layer for an audible one-shot effect, none for a silent one, or one looping layer per channel.</param>
public sealed record InstalledSoundEffectClip(
    SoundEffectId Sound,
    SoundEffectClipPolicy Policy,
    long? ActiveFrames,
    IReadOnlyList<InstalledSoundEffectLayer> Layers)
{
    /// <summary>The entry the game plays; a method, so the manifest does not store it.</summary>
    public SoundEffectClipEntry PlayedEntry() =>
        new(Sound, Policy, ActiveFrames, [.. Layers.Select(layer => layer.PlayedLayer())]);
}
