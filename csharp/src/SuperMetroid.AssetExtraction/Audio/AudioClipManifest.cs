using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The installed pre-rendered audio's index (#1305): every music track of every data set and every sound effect, each
/// with its clip files. The catalog the clips were rendered from is the import's data and is not installed (#1693).
/// </summary>
/// <param name="FormatVersion">Must equal <see cref="CurrentFormatVersion"/>.</param>
/// <param name="SampleRate">The clips' rate in stereo frames per second: 32,040, one update's clock samples sixty times.</param>
/// <param name="InitialBeat">How long the music's beat lasts at power-on, before any song has played.</param>
/// <param name="Music">One clip per song of each clip set.</param>
/// <param name="SoundEffects">One entry per sound effect.</param>
public sealed record AudioClipManifest(
    int FormatVersion,
    int SampleRate,
    BeatLength InitialBeat,
    IReadOnlyList<MusicClipEntry> Music,
    IReadOnlyList<InstalledSoundEffectClip> SoundEffects)
{
    /// <summary>
    /// The manifest schema this build reads and writes. Version 2 (#1553) names each song by its part in its data set
    /// and each sound's library by name, where version 1 held the cartridge's track and library numbering. Version 3
    /// (#1693) names each sound by its identity alone, where version 2 held its library's request number, and records no
    /// source catalog identity, clip digest or missing instrument sources. Version 4 (#1725) times the music's beat in
    /// clip frames: the power-on beat and each tempo segment hold how long the beat lasts, where version 3 held the sound
    /// driver's tempo values, and each sound-effect layer lists its note starts as <c>noteStarts</c>. Version 5 (#1749) records no voice count in a
    /// sound's policy. Version 6 (#1748) names each song for what it is, where version 5 named its part in its data set,
    /// and names the boss data sets for their content. Version 7 (#2057) holds each sound effect once, rendered from the
    /// shared sounds' samples, where version 6 held one per data set and the sounds a data set could not render, and
    /// records no note starts. Version 8 (#2475) names the Mama Turtle's spin for her, where version 7 named it by the
    /// original's name for her.
    /// </summary>
    public const int CurrentFormatVersion = 8;
}
