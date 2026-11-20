namespace SuperMetroid.Core.Audio;

/// <summary>
/// What a room state does with the music track when Samus enters (#1410): keep whatever plays, play one of the loaded
/// data set's songs, play the loaded set's area theme (#1748), or silence the music.
/// </summary>
public abstract record RoomMusicTrack
{
    /// <summary>Closes the hierarchy to the choices declared here.</summary>
    private RoomMusicTrack()
    {
    }

    /// <summary>Keeps the track that plays, through every door into the room (the Ceres escape relies on it).</summary>
    public sealed record Inherit : RoomMusicTrack;

    /// <summary>Plays <paramref name="Track"/>, queued only when it is not already playing or the room changes the data set.</summary>
    /// <param name="Track">The track the room plays.</param>
    public sealed record Play(MusicTrack Track) : RoomMusicTrack;

    /// <summary>
    /// Plays the loaded data set's area theme (<see cref="AreaThemeDefinitions"/>), which a room that keeps the loaded
    /// set chooses: the song depends on the set loaded when the request plays. Queued only when the theme of the set
    /// loaded now is not already playing, or the room changes the data set.
    /// </summary>
    public sealed record PlayAreaTheme : RoomMusicTrack;

    /// <summary>
    /// Silences the music, queued on every entry: the choice never equals the playing song, so the stop is always
    /// queued.
    /// </summary>
    public sealed record Silence : RoomMusicTrack;

    /// <summary>
    /// The music request that carries out the choice when it is queued: its track, or a stop. Room loading never queues
    /// an inherited track; a saved game queues the room's choice directly, and an inherited track stops the music there.
    /// </summary>
    internal MusicRequest Request => this switch
    {
        Play play => new MusicRequest.PlayTrack(play.Track),
        PlayAreaTheme => MusicRequest.AreaTheme,
        _ => MusicRequest.Stop,
    };

    /// <summary>The choice that keeps the playing track.</summary>
    public static RoomMusicTrack Inherited { get; } = new Inherit();

    /// <summary>The choice that plays the loaded data set's area theme.</summary>
    public static RoomMusicTrack PlaysAreaTheme { get; } = new PlayAreaTheme();

    /// <summary>The choice that silences the music.</summary>
    public static RoomMusicTrack Silenced { get; } = new Silence();

    /// <summary>The choice that plays <paramref name="track"/> of the loaded data set.</summary>
    /// <param name="track">The song the room plays.</param>
    public static RoomMusicTrack Plays(MusicTrack track) => new Play(track);
}
