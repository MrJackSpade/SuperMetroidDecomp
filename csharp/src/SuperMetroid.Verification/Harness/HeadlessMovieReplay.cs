using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// The verifier's host for a <see cref="ConvertedMovieSession"/>: it steps the session's game directly, one frame per
/// input, with no frame timer, and prints the session's final report.
/// </summary>
internal static class HeadlessMovieReplay
{
    /// <summary>Replays <paramref name="movie"/> against the native trace in <paramref name="traceDirectory"/>.</summary>
    /// <param name="movie">The movie to replay.</param>
    /// <param name="traceDirectory">The movie's compact native trace.</param>
    /// <param name="traceFromUpdate">Diagnostic only: from this update on, print port and native state field by field.</param>
    internal static void Run(ReplayMovieId movie, string traceDirectory, int? traceFromUpdate = null)
    {
        using var session = ConvertedMovieSession.Open(movie, traceDirectory, traceFromUpdate);
        RunToEnd(session);
        Console.WriteLine(session.Finish());
    }

    /// <summary>Runs every remaining frame of <paramref name="session"/>, comparing each completed one.</summary>
    /// <param name="session">The session to run.</param>
    internal static void RunToEnd(ConvertedMovieSession session)
    {
        while (session.NextInput() is { } input)
        {
            try
            {
                session.Game.Step(input);
            }
            catch (Exception exception) when (session.NamedProductionFailure(exception) is { } named)
            {
                throw named;
            }
            session.FrameCompleted();
        }
    }
}
