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

    /// <summary>The track that plays once the request is carried out, or null when none plays.</summary>
    internal abstract MusicTrack? TrackPlayed { get; }

    /// <summary>Plays <paramref name="Track"/> of the loaded data set.</summary>
    /// <param name="Track">The track to play.</param>
    public sealed record PlayTrack(MusicTrack Track) : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayed => Track;
    }

    /// <summary>Stops the music.</summary>
    public sealed record StopMusic : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayed => null;
    }

    /// <summary>Changes the music data set to <paramref name="DataSet"/>, which stops the music.</summary>
    /// <param name="DataSet">The data set to load.</param>
    public sealed record ChangeDataSet(MusicDataSet DataSet) : MusicRequest
    {
        /// <inheritdoc/>
        internal override MusicTrack? TrackPlayed => null;
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
