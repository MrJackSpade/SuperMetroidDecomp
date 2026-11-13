using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The installed pre-rendered audio's index (#1305): every music track and every sound effect in every data set, each
/// with its clip files. The catalog the clips were rendered from is the import's data and is not installed (#1693).
/// </summary>
/// <param name="FormatVersion">Must equal <see cref="CurrentFormatVersion"/>.</param>
/// <param name="SampleRate">The clips' rate in stereo frames per second: 32,040, one update's clock samples sixty times.</param>
/// <param name="InitialBeat">How long the music's beat lasts at power-on, before any song has played.</param>
/// <param name="Music">One clip per song of each clip set.</param>
/// <param name="SoundEffects">One entry per renderable (data set, effect).</param>
/// <param name="Unrenderable">The (data set, effect) pairs with no clip.</param>
public sealed record AudioClipManifest(
    int FormatVersion,
    int SampleRate,
    BeatLength InitialBeat,
    IReadOnlyList<MusicClipEntry> Music,
    IReadOnlyList<InstalledSoundEffectClip> SoundEffects,
    IReadOnlyList<UnrenderableSoundEffect> Unrenderable)
{
    /// <summary>
    /// The manifest schema this build reads and writes. Version 2 (#1553) names each song by its part in its data set
    /// and each sound's library by name, where version 1 held the cartridge's track and library numbering. Version 3
    /// (#1693) names each sound by its identity alone, where version 2 held its library's request number, and records no
    /// source catalog identity, clip digest or missing instrument sources. Version 4 (#1725) times the music's beat in
    /// clip frames: the power-on beat and each tempo segment hold how long the beat lasts, where version 3 held the sound
    /// driver's tempo values, and each sound-effect layer lists its note starts as <c>noteStarts</c>.
    /// </summary>
    public const int CurrentFormatVersion = 4;
}
