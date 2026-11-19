namespace SuperMetroid.Core.Audio;

/// <summary>Reads and writes a <see cref="MusicTrack"/> in an installed document by its installed key (#2520).</summary>
internal sealed class MusicTrackJsonConverter() : InstalledKeyJsonConverter<MusicTrack>(MusicTrackKeys.Installed);
