using SuperMetroid.Core.Frontend;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>One movie's own checks, run beside the shared desynchronization comparison of its converted replay.</summary>
internal interface IConvertedMovieChecks
{
    /// <summary>This movie's own checks of the port after checkpointed update <paramref name="update"/>.</summary>
    /// <returns>Each mismatch, reported with the shared comparison's.</returns>
    IEnumerable<string> Compare(SuperMetroidGame game, int update, NativeCheckpointMemory native);

    /// <summary>This movie's closing checks once every update matched.</summary>
    /// <returns>The lines this movie adds to the replay's final report.</returns>
    IEnumerable<string> Finish();
}
