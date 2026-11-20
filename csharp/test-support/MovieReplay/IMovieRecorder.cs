namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Records a replayed movie's picture and sound; the session hands it every game it creates.</summary>
internal interface IMovieRecorder
{
    /// <summary>Whether the recording keeps the game's sound, so the recorded game is built mixing the installed clips.</summary>
    bool RecordsAudio { get; }
}
