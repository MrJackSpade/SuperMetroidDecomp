using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.MovieReplay.Verification;

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
            RunFrame(session, () => session.Game.Step(input));
            session.FrameCompleted();
        }
    }

    /// <summary>
    /// Records the frames of <paramref name="movie"/> that <paramref name="video"/>'s segment covers, comparing every
    /// frame up to the segment's end as the whole replay does.
    /// </summary>
    /// <param name="movie">The movie to record.</param>
    /// <param name="video">The segment to record.</param>
    internal static void Record(ReplayMovieId movie, MovieReplayVideo video)
    {
        using var session = ConvertedMovieSession.Open(movie, ReplayMovieFixture.Of(movie).TraceDirectory, video: video);
        while (!video.Done && session.NextInput() is { } input)
        {
            MovieReplayPosition position = session.Position
                ?? throw new InvalidOperationException("The replay handed a frame without saying where it comes from.");
            RunFrame(session, () => video.Step(session.Game, input, position));
            session.FrameCompleted();
        }
        video.Complete();
    }

    /// <summary>The number of frames <paramref name="movie"/>'s replay runs, which is the length of its video.</summary>
    /// <param name="movie">The movie to count.</param>
    internal static long FrameCount(ReplayMovieId movie)
    {
        using var session = ConvertedMovieSession.Open(movie);
        long frames = 0;
        while (session.NextInput() is { } input)
        {
            RunFrame(session, () => session.Game.Step(input));
            session.FrameCompleted();
            frames++;
        }
        Console.WriteLine(session.Finish());
        return frames;
    }

    private static void RunFrame(ConvertedMovieSession session, Action frame)
    {
        try
        {
            frame();
        }
        catch (Exception exception) when (session.NamedProductionFailure(exception) is { } named)
        {
            throw named;
        }
    }
}
