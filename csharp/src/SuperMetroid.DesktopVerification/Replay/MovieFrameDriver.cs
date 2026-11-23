using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop.Window;
using SuperMetroid.TestSupport.MovieReplay;

namespace SuperMetroid.DesktopVerification.Replay;

/// <summary>
/// Drives the Direct3D 11 host from a converted SMV movie session. The session owns the movie's one
/// initial-state import, its input timeline and its checkpoint comparison; the host owns the frame loop,
/// presentation and audio. The movie's outcome, its final report or its first failure, completes
/// <see cref="Completion"/>, and a finished or failed movie supplies no further input, which stops the host.
/// </summary>
internal sealed class MovieFrameDriver : IHostFrameDriver
{
    /// <summary>The converted movie whose game and inputs the host runs.</summary>
    private readonly ConvertedMovieSession session;
    /// <summary>Completes with the session's final report, or faults with the first divergence or failure.</summary>
    private readonly TaskCompletionSource<string> outcome = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Whether the host already started the session's game.</summary>
    private bool started;
    /// <summary>The updates to run before stopping, or null to run the whole movie.</summary>
    private readonly int? until;
    /// <summary>Times each update from the host finishing the one before.</summary>
    private readonly System.Diagnostics.Stopwatch sinceLast = new();

    /// <summary>
    /// How long an update may take the host before a timed run reports it (#2619): over two frames at 60 Hz, a dropped
    /// frame a player sees.
    /// </summary>
    private static readonly TimeSpan SlowUpdate = TimeSpan.FromMilliseconds(34);

    /// <summary>Drives the host from <paramref name="session"/>, stopping after <paramref name="until"/> updates when given.</summary>
    internal MovieFrameDriver(ConvertedMovieSession session, int? until, bool measure = false)
    {
        this.session = Ensure.NotNull(session);
        this.until = until;
        measured = measure ? [] : null;
    }

    /// <summary>The movie's final report line, or the exception that ended it.</summary>
    internal Task<string> Completion => outcome.Task;

    /// <inheritdoc/>
    public SuperMetroidGame Game => session.Game;

    /// <inheritdoc/>
    public int InputsSupplied { get; private set; }

    /// <inheritdoc/>
    public string Progress => $"movie update {InputsSupplied}";

    /// <inheritdoc/>
    /// <remarks>
    /// A movie replay checks gameplay, not timing, so it runs as fast as the host can step and present (#1511); a measured
    /// replay presents every frame (#2653).
    /// </remarks>
    public HostFramePacing Pacing => measured is null ? HostFramePacing.Unpaced : HostFramePacing.EachFramePresented;

    /// <summary>Each update's timing and place in a measured replay (#2653); null for any other.</summary>
    private readonly List<MeasuredUpdate>? measured;

    /// <summary>The host's latest published frame's sequence, read as each update completes in a measured replay.</summary>
    internal Func<long>? DisplaySequence { get; set; }

    /// <summary>When the update now running took its input.</summary>
    private long updateStarted;

    /// <summary>Each update a measured replay ran, in order.</summary>
    /// <exception cref="InvalidOperationException">The replay is not measured.</exception>
    internal IReadOnlyList<MeasuredUpdate> MeasuredUpdates => measured ?? throw new InvalidOperationException("The replay is not measured.");

    /// <summary>Hands the host the session's game, which the session created by the movie's one initial-state import.</summary>
    /// <remarks>The host's installed content is not used: the session imports the movie's initial state itself. A
    /// second start would need a second import, which full-movie parity forbids, so a restart fails.</remarks>
    public SuperMetroidGame Start(HostSessionStart start)
    {
        Ensure.NotNull(start);
        if (started)
            throw new InvalidOperationException("A movie replay imports its initial state once; the host cannot restart it.");
        started = true;
        return session.Game;
    }

    /// <summary>The session's next input; null once the movie finished or failed, which completes the outcome on the movie's end.</summary>
    public ControllerButton? NextInput()
    {
        if (outcome.Task.IsCompleted)
            return null;
        if (until is { } last && InputsSupplied >= last)
        {
            outcome.SetResult($"Stopped after {InputsSupplied} updates.");
            return null;
        }
        ControllerButton? input = session.NextInput();
        if (input is null)
        {
            CompleteWithReport();
            return null;
        }
        InputsSupplied++;
        updateStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        return input;
    }

    /// <summary>Lets the session compare the frame the host just ran; its first divergence ends the movie.</summary>
    public void FrameCompleted(FrontendFrame frame)
    {
        // The host runs its first frame as it is built, before the replay hands it the frame sequence and starts the render
        // worker's log; the measurement starts with the frames after it.
        if (measured is not null && DisplaySequence is { } sequence)
        {
            SuperMetroidGame game = session.Game;
            measured.Add(new MeasuredUpdate(InputsSupplied, session.Position?.SourceFrame, frame.Phase,
                game.GameplayActiveRoom?.Name, game.GameplayActiveRoomState?.Name,
                System.Diagnostics.Stopwatch.GetElapsedTime(updateStarted).TotalMilliseconds, sequence()));
        }
        // A timed run reports each update the host took long over, with where the movie stood (#2619).
        else if (until is not null && sinceLast.IsRunning && sinceLast.Elapsed > SlowUpdate)
            Console.WriteLine($"slow update {InputsSupplied} (source frame {session.Position?.SourceFrame}, {frame.Phase}): {sinceLast.Elapsed.TotalMilliseconds:F0} ms");
        sinceLast.Restart();
        try
        {
            session.FrameCompleted();
        }
        catch (Exception divergence)
        {
            // The divergence is the movie's result, not a recovered error: the outcome faults with it,
            // the next NextInput stops the host, and the command rethrows it at its process boundary.
            outcome.SetException(divergence);
        }
    }

    /// <summary>Completes the outcome with the session's final report, or with the failure that producing it raised.</summary>
    private void CompleteWithReport()
    {
        string report;
        try
        {
            report = session.Finish();
        }
        catch (Exception failure)
        {
            // As in FrameCompleted: the failure is the movie's result and the command rethrows it.
            outcome.SetException(failure);
            return;
        }
        outcome.SetResult(report);
    }
}
