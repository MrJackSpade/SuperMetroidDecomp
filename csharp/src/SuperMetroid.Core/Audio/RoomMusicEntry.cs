namespace SuperMetroid.Core.Audio;

/// <summary>The music as a room's entry finds it (#2553), which decides whether the room's track choice is queued.</summary>
/// <param name="DataSetDiffers">Whether the room changes the music data set.</param>
/// <param name="Playing">The track that plays, or null when none does.</param>
/// <param name="Loaded">The music data set loaded.</param>
internal readonly partial record struct RoomMusicEntry(bool DataSetDiffers, MusicTrack? Playing, MusicDataSet Loaded);
