namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The persistent audio override (#1305 Q3): replacement clips kept outside the replaceable game
/// content, so they survive repair and updates. Each replaces a stock clip by identity.
/// </summary>
/// <param name="FormatVersion">Must equal <see cref="CurrentFormatVersion"/>.</param>
/// <param name="Clips">The replacements, at most one per identity.</param>
public sealed record AudioClipOverrideManifest(int FormatVersion, IReadOnlyList<AudioClipOverride> Clips)
{
    /// <summary>The override schema this build reads and writes.</summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>The override manifest's file name in the override directory.</summary>
    public const string FileName = "audio-clip-overrides.json";
}
