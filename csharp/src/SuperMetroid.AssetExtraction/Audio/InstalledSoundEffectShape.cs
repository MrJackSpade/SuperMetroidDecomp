using System.Text.Json.Serialization;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// A sound effect's shape as the clip manifest stores it (#2560): a one-shot that ends, or a never-ending effect whose
/// channels each loop, named in the manifest by its <c>shape</c>.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "shape")]
[JsonDerivedType(typeof(InstalledOneShotSoundEffect), "oneShot")]
[JsonDerivedType(typeof(InstalledLoopingSoundEffect), "looping")]
public abstract partial record InstalledSoundEffectShape
{
    /// <summary>The shape the game plays; a method, so the manifest does not store it.</summary>
    public abstract SoundEffectClipShape PlayedShape();

    /// <summary>Every clip file the shape plays.</summary>
    public abstract IEnumerable<AudioClipFile> Clips();
}
