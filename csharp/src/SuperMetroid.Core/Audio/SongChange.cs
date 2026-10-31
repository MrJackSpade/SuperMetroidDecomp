namespace SuperMetroid.Core.Audio;

/// <summary>A song change waiting for the music's next beat (#1478): play <paramref name="Track"/>, or stop for none.</summary>
/// <param name="Track">The track of the loaded clip set to play, or null to stop the music.</param>
internal sealed record SongChange(MusicTrack? Track);
