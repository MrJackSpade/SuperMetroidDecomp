using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>A never-ending sound effect, as the clip manifest stores it (#2560): one looping layer per channel.</summary>
/// <param name="Layers">The channels' layers.</param>
public sealed record InstalledLoopingSoundEffect(IReadOnlyList<InstalledLoopingSoundEffectLayer> Layers) : InstalledSoundEffectShape
{
    /// <inheritdoc/>
    public override SoundEffectClipShape PlayedShape() =>
        new LoopingSoundEffectClip([.. Layers.Select(layer => new LoopingSoundEffectLayer(layer.Clip, layer.LoopStart))]);

    /// <inheritdoc/>
    public override IEnumerable<AudioClipFile> Clips() => Layers.Select(layer => layer.Clip);
}
