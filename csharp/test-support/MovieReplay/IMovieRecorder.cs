namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Records a replayed movie's picture and sound; the session builds every game it creates with its audio.</summary>
internal interface IMovieRecorder
{
    /// <summary>What a recorded game's audio produces: a new game and the one a console reset rebuilt are built with it.</summary>
    SuperMetroid.Core.Audio.GameAudioOutput Audio { get; }
}
