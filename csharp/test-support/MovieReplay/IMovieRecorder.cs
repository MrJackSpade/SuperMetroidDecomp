namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Records a replayed movie's picture and sound; the session builds every game it creates with its audio.</summary>
internal interface IMovieRecorder
{
    /// <summary>The clips a recorded game's audio mixes: a new game and the one a console reset rebuilt are built with them.</summary>
    SuperMetroid.Core.Audio.ClipAudioLibrary Audio { get; }
}
