using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>A sound effect that ends, as the clip manifest stores it (#2560).</summary>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets.</param>
/// <param name="Clip">The rendered sound, or null for a silent effect, which still holds its library until its end.</param>
public sealed record InstalledOneShotSoundEffect(long ActiveFrames, AudioClipFile? Clip) : InstalledSoundEffectShape
{
    /// <inheritdoc/>
    public override SoundEffectClipShape PlayedShape() => new OneShotSoundEffectClip(ActiveFrames, Clip);

    /// <inheritdoc/>
    public override IEnumerable<AudioClipFile> Clips() => Clip is { } clip ? [clip] : [];
}
