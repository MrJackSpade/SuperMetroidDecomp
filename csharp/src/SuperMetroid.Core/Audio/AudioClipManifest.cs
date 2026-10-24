namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed pre-rendered audio (#1305): every music track and every sound effect in every
/// data set, rendered from the extracted audio catalog named by
/// <see cref="SourceContentIdentity"/>.
/// </summary>
/// <param name="FormatVersion">Must equal <see cref="CurrentFormatVersion"/>.</param>
/// <param name="SourceContentIdentity">The content identity of the extracted audio catalog the clips were rendered from.</param>
/// <param name="SampleRate">The clips' rate in stereo frames per second: the native S-DSP rate.</param>
/// <param name="Music">One clip per (bank, track).</param>
/// <param name="SoundEffects">One entry per renderable (data set, effect).</param>
/// <param name="Unrenderable">The (data set, effect) pairs with no clip.</param>
public sealed record AudioClipManifest(
    int FormatVersion,
    string SourceContentIdentity,
    int SampleRate,
    IReadOnlyList<MusicClipEntry> Music,
    IReadOnlyList<SoundEffectClipEntry> SoundEffects,
    IReadOnlyList<UnrenderableSoundEffect> Unrenderable)
{
    /// <summary>The manifest schema this build reads and writes.</summary>
    public const int CurrentFormatVersion = 1;
}
