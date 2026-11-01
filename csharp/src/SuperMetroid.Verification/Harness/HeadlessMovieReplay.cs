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
    /// <param name="video">When supplied, records every update's picture and sound as the desktop host presents it.</param>
    internal static void Run(ReplayMovieId movie, string traceDirectory, int? traceFromUpdate = null, MovieReplayVideo? video = null)
    {
        using var session = ConvertedMovieSession.Open(movie, traceDirectory, traceFromUpdate, video);
        RunToEnd(session, video);
        Console.WriteLine(session.Finish());
    }

    /// <summary>Runs every remaining frame of <paramref name="session"/>, comparing each completed one.</summary>
    /// <param name="session">The session to run.</param>
    /// <param name="video">When supplied, each frame is run through it so it records the frame's picture and sound.</param>
    internal static void RunToEnd(ConvertedMovieSession session, MovieReplayVideo? video = null)
    {
        while (session.NextInput() is { } input)
        {
            try
            {
                // Recording presents each update exactly as the host does; otherwise frames are not rasterized.
                if (video is null) session.Game.Step(input);
                else video.Step(session.Game, input);
            }
            catch (Exception exception) when (session.NamedProductionFailure(exception) is { } named)
            {
                throw named;
            }
            session.FrameCompleted();
        }
    }
}
