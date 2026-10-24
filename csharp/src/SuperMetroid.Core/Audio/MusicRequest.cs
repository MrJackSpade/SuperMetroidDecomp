namespace SuperMetroid.Core.Audio;

/// <summary>
/// One request the game puts on its music queue (#1410): play a track of the loaded data set, stop the music, or change
/// the music data set. The queue holds each with its delay and dispatches it when the delay expires.
/// </summary>
public abstract record MusicRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private MusicRequest()
    {
    }

    /// <summary>Plays <paramref name="Track"/> of the loaded data set.</summary>
    /// <param name="Track">The track to play.</param>
    public sealed record PlayTrack(MusicTrack Track) : MusicRequest;

    /// <summary>Stops the music. An empty queue position holds this request.</summary>
    public sealed record StopMusic : MusicRequest;

    /// <summary>Changes the music data set to <paramref name="DataSet"/>, which stops the music.</summary>
    /// <param name="DataSet">The data set to load.</param>
    public sealed record ChangeDataSet(MusicDataSet DataSet) : MusicRequest;

    /// <summary>The request that stops the music.</summary>
    public static MusicRequest Stop { get; } = new StopMusic();

    /// <summary>The request that plays track <paramref name="track"/> of the loaded data set.</summary>
    /// <param name="track">The track's number, 1 through <see cref="MusicTrack.LastNumber"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="track"/> is outside 1 through <see cref="MusicTrack.LastNumber"/>.</exception>
    public static MusicRequest SelectTrack(int track) => new PlayTrack(new MusicTrack(track));

    /// <summary>The request that plays <paramref name="track"/>, or stops the music for none.</summary>
    /// <param name="track">The track to play, or null to stop.</param>
    public static MusicRequest SelectTrackOrStop(MusicTrack? track) => track is MusicTrack playing ? new PlayTrack(playing) : Stop;

    /// <summary>The request that loads music data set <paramref name="dataSet"/>.</summary>
    /// <param name="dataSet">The data set to load.</param>
    public static MusicRequest LoadDataSet(MusicDataSet dataSet) => new ChangeDataSet(dataSet);
}
