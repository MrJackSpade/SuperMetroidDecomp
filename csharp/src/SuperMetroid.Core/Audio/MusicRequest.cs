namespace SuperMetroid.Core.Audio;

/// <summary>
/// One request the game puts on its music queue (#1410): play a song of the loaded data set, stop the music, or change
/// the music data set. A room's area theme (#1748) is queued as the song it is (<see cref="AudioQueues.ScheduleRoomTrack"/>). The queue holds each with its delay and carries it out when the delay expires.
/// </summary>
public abstract record MusicRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private MusicRequest()
    {
    }

    /// <summary>The track that plays once the request is carried out with <paramref name="dataSet"/> loaded, or null when none plays.</summary>
    /// <param name="dataSet">The music data set loaded as the request plays.</param>
    internal abstract MusicTrack? TrackPlayedWith(MusicDataSet? dataSet);

    /// <summary>Plays <paramref name="Track"/> of the loaded data set.</summary>
    /// <param name="Track">The track to play.</param>
    public sealed record PlayTrack(MusicTrack Track) : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayedWith(MusicDataSet? dataSet) => Track;
    }

<<<<<<< ours
||||||| base
    /// <summary>
    /// Plays the area theme (<see cref="AreaThemeDefinitions"/>) of the data set loaded when the request plays, which a room
    /// that keeps the loaded set asks for (#1748).
    /// </summary>
    public sealed record PlayAreaTheme : MusicRequest;

=======
    /// <summary>
    /// Plays the area theme (<see cref="AreaThemeDefinitions"/>) of the data set loaded when the request plays, which a room
    /// that keeps the loaded set asks for (#1748).
    /// </summary>
    public sealed record PlayAreaTheme : MusicRequest
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">No data set is loaded, so no area theme can play.</exception>
        internal override MusicTrack? TrackPlayedWith(MusicDataSet? dataSet) => dataSet is MusicDataSet loaded
            ? AreaThemeDefinitions.Themes[loaded]
            : throw new InvalidOperationException("An area theme was requested with no music data set loaded.");
    }

>>>>>>> theirs
    /// <summary>Stops the music.</summary>
    public sealed record StopMusic : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayedWith(MusicDataSet? dataSet) => null;
    }

    /// <summary>Changes the music data set to <paramref name="DataSet"/>, which stops the music.</summary>
    /// <param name="DataSet">The data set to load.</param>
    public sealed record ChangeDataSet(MusicDataSet DataSet) : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayedWith(MusicDataSet? dataSet) => null;
    }

    /// <summary>The request that stops the music.</summary>
    public static MusicRequest Stop { get; } = new StopMusic();

    /// <summary>The request that plays <paramref name="track"/> of the loaded data set.</summary>
    /// <param name="track">The song to play.</param>
    public static MusicRequest SelectTrack(MusicTrack track) => new PlayTrack(track);

    /// <summary>The request that plays <paramref name="track"/>, or stops the music for none.</summary>
    /// <param name="track">The track to play, or null to stop.</param>
    public static MusicRequest SelectTrackOrStop(MusicTrack? track) => track is MusicTrack playing ? new PlayTrack(playing) : Stop;

    /// <summary>The request that loads music data set <paramref name="dataSet"/>.</summary>
    /// <param name="dataSet">The data set to load.</param>
    public static MusicRequest LoadDataSet(MusicDataSet dataSet) => new ChangeDataSet(dataSet);
}
