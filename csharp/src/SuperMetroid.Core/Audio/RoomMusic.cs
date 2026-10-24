namespace SuperMetroid.Core.Audio;

/// <summary>A room state's music (#1410): the data set it loads, if any, and what it does with the track.</summary>
/// <param name="DataSet">The data set the room loads when it is not already loaded, or null to keep the loaded one.</param>
/// <param name="Track">What the room does with the music track.</param>
public sealed record RoomMusic(MusicDataSet? DataSet, RoomMusicTrack Track)
{
    /// <summary>The music of a room that keeps both the loaded data set and the playing track.</summary>
    public static RoomMusic Inherited { get; } = new(null, RoomMusicTrack.Inherited);
}
