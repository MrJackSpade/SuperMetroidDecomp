using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A supplied movie's converted replay, driven one frame at a time by whichever host runs the game: the headless
/// verifier steps <see cref="Game"/> itself, the Direct3D 11 host lets its own frame loop step it. The host asks
/// <see cref="NextInput"/> for each frame's controller input, runs that frame, and reports it with
/// <see cref="FrameCompleted"/>; the session compares every checkpointed update with the native recording and throws
/// at the first divergence. A frame is one game step: an update's input, or one message-box frame inside an update.
/// </summary>
/// <remarks>
/// The movie's one initial-state import happens in <see cref="Open(ReplayMovieId)"/>; nothing native is fed back into
/// the game afterwards (docs/smv-gameplay-parity.md).
/// </remarks>
internal sealed class ConvertedMovieSession : IDisposable
{
    /// <summary>The movie's replay.</summary>
    private readonly IConvertedMovieReplay replay;

    /// <summary>The replay's frame sequence, positioned at a frame boundary or on the frame the host runs.</summary>
    private readonly IEnumerator<ControllerButton?> frames;

    /// <summary>Whether the host is running the frame <see cref="NextInput"/> returned.</summary>
    private bool frameRunning;

    /// <summary>Whether the movie has no frames left.</summary>
    private bool finished;

    /// <summary>Takes ownership of <paramref name="replay"/> and runs its opening import.</summary>
    private ConvertedMovieSession(IConvertedMovieReplay replay)
    {
        this.replay = replay;
        frames = replay.Frames().GetEnumerator();
        try
        {
            AdvanceToBoundary("opening import");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>Opens a movie's converted timeline and native checkpoints and creates its game (the one initial-state import).</summary>
    internal static ConvertedMovieSession Open(ReplayMovieId movie) =>
        Open(movie, ReplayMovieFixture.Of(movie).TraceDirectory);

    /// <summary>
    /// Opens a movie against the native trace in <paramref name="traceDirectory"/> and creates its game (the one
    /// initial-state import).
    /// </summary>
    /// <param name="movie">The movie to replay.</param>
    /// <param name="traceDirectory">The movie's compact native trace.</param>
    /// <param name="traceFromUpdate">
    /// Diagnostic only: from this update on, print port and native state field by field so a divergence can be read.
    /// Comparison and failure are unchanged. The Ridley showcase replay has no such trace.
    /// </param>
    /// <param name="video">When supplied, the host records every update's picture and sound with it; power-on movies only.</param>
    internal static ConvertedMovieSession Open(ReplayMovieId movie, string traceDirectory, int? traceFromUpdate = null, MovieReplayVideo? video = null)
    {
        if (movie == ReplayMovieId.Ridley)
        {
            if (traceFromUpdate is not null || video is not null)
                throw new NotSupportedException("The Ridley showcase replay has no update trace or video export.");
            return new ConvertedMovieSession(new RidleyShowcaseReplay(Path.GetFullPath(traceDirectory)));
        }
        ReplayMovie recording = ReplayMovieFixture.Of(movie).Load();
        IConvertedMovieChecks? checks = movie switch
        {
            ReplayMovieId.LowPercent13 => new LowPercentIntroReplayChecks(),
            ReplayMovieId.MotherBrainGlitch => new MotherBrainCryChecks(),
            ReplayMovieId.Full100 or ReplayMovieId.Drunk or ReplayMovieId.Lowc => null,
            ReplayMovieId.Ridley => throw new InvalidOperationException("The Ridley showcase replay is opened above."),
            _ => throw new ArgumentOutOfRangeException(nameof(movie), movie, "Undefined replay movie."),
        };
        NativeMovieCheckpoints checkpoints = NativeMovieCheckpoints.Open(traceDirectory, recording.Bytes);
        ConvertedTimelineReplay timeline;
        try
        {
            timeline = new ConvertedTimelineReplay(recording, checkpoints, traceFromUpdate, checkpoints.Updates.Count, checks, video);
        }
        catch
        {
            checkpoints.Dispose();
            throw;
        }
        return new ConvertedMovieSession(timeline);
    }

    /// <summary>
    /// Opens the 100% movie against a private full native capture and replays it only up to
    /// <paramref name="lastUpdate"/>, which must have a native checkpoint; for diagnostics that need complete WRAM.
    /// </summary>
    /// <returns>The session and its timeline, whose <see cref="ConvertedTimelineReplay.Memory"/> follows the replay.</returns>
    internal static FullCaptureReplaySession OpenFullCapture(ReplayMovieId movie, string fullCaptureDirectory, int lastUpdate)
    {
        if (movie == ReplayMovieId.Ridley)
            throw new NotSupportedException("The Ridley showcase replay reads only its compact trace.");
        ReplayMovie recording = ReplayMovieFixture.Of(movie).Load();
        NativeMovieCheckpoints checkpoints = NativeMovieCheckpoints.OpenFullCapture(fullCaptureDirectory, recording.Bytes);
        ConvertedTimelineReplay timeline;
        try
        {
            timeline = new ConvertedTimelineReplay(recording, checkpoints, traceFromUpdate: null, lastUpdate, movieChecks: null, video: null);
        }
        catch
        {
            checkpoints.Dispose();
            throw;
        }
        return new FullCaptureReplaySession(new ConvertedMovieSession(timeline), timeline);
    }

    /// <summary>The game the host steps; a console reset in the movie replaces it.</summary>
    internal SuperMetroidGame Game => replay.Game;

    /// <summary>The next frame's controller input, or null when the movie is finished. Sets this frame's recorded-evidence expectations.</summary>
    internal ControllerButton? NextInput()
    {
        if (frameRunning)
            throw new InvalidOperationException("The host asked for the next frame before completing the previous one.");
        if (finished)
            return null;
        if (!frames.MoveNext())
        {
            finished = true;
            return null;
        }
        ControllerButton input = frames.Current ??
            throw new InvalidOperationException("The movie replay reached a frame boundary where a frame was due.");
        frameRunning = true;
        return input;
    }

    /// <summary>Completes the frame the host just ran with the last input: message-box bookkeeping, console resets, and the comparison at each checkpointed update. Throws on the first divergence, exactly as today.</summary>
    internal void FrameCompleted()
    {
        if (!frameRunning)
            throw new InvalidOperationException("The host completed a frame the session did not hand it.");
        frameRunning = false;
        AdvanceToBoundary("completed frame");
    }

    /// <summary>The final report line once NextInput returned null (updates, source frames, uncheckpointed updates), after asserting the checkpoints are exhausted.</summary>
    internal string Finish() => finished
        ? replay.Finish()
        : throw new InvalidOperationException("The movie still has frames to run.");

    /// <summary>
    /// The failure to report for <paramref name="exception"/>, thrown by production code while the host ran the frame
    /// <see cref="NextInput"/> returned: an update's input frame names its update and source frame, keeping the original
    /// as the inner exception. <see langword="null"/> when the original should propagate unchanged.
    /// </summary>
    internal Exception? NamedProductionFailure(Exception exception) => replay.NamedProductionFailure(exception);

    /// <summary>Closes the frame sequence and the native checkpoint stream.</summary>
    public void Dispose()
    {
        frames.Dispose();
        replay.Dispose();
    }

    /// <summary>Runs the replay to its next frame boundary: the frame just run is complete and compared.</summary>
    private void AdvanceToBoundary(string after)
    {
        if (!frames.MoveNext() || frames.Current is not null)
            throw new InvalidOperationException($"The movie replay did not reach a frame boundary after its {after}.");
    }
}
