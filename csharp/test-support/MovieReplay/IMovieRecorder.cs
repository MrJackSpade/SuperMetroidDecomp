namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Records a replayed movie's picture and sound; the session hands it every game it creates.</summary>
internal interface IMovieRecorder
{
    /// <summary>Starts recording <paramref name="game"/>, a new game or the one a console reset rebuilt.</summary>
    void Attach(SuperMetroid.Core.Frontend.SuperMetroidGame game);
}
