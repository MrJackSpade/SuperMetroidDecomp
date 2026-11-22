using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One sound effect rendered dry once, as the clip manifest stores it (#1305, #2057): the entry the game plays, whichever
/// music data set is loaded. Frame zero is the request check that accepted the request.
/// </summary>
/// <param name="Sound">The sound effect, whose library plays it.</param>
/// <param name="Policy">How the library accepts and starts the request.</param>
/// <param name="Shape">Whether the effect is a one-shot that ends or a never-ending loop, with the clips each plays (#2560).</param>
public sealed partial record InstalledSoundEffectClip(
    SoundEffectId Sound,
    SoundEffectClipPolicy Policy,
    InstalledSoundEffectShape Shape)
{
    /// <summary>The entry the game plays; a method, so the manifest does not store it.</summary>
    public SoundEffectClipEntry PlayedEntry() => new(Sound, Policy, Shape.PlayedShape());
}
