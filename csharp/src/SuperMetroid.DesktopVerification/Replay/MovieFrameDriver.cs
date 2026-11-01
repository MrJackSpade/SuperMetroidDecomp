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
    /// <summary>Inputs the session supplied to the host.</summary>
    private int inputsSupplied;

    /// <summary>Drives the host from <paramref name="session"/>.</summary>
    internal MovieFrameDriver(ConvertedMovieSession session) => this.session = Ensure.NotNull(session);

    /// <summary>The movie's final report line, or the exception that ended it.</summary>
    internal Task<string> Completion => outcome.Task;

    /// <inheritdoc/>
    public SuperMetroidGame Game => session.Game;

    /// <inheritdoc/>
    public int InputsSupplied => inputsSupplied;

    /// <inheritdoc/>
    public string Progress => $"movie update {inputsSupplied}";

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
        ControllerButton? input = session.NextInput();
        if (input is null)
        {
            CompleteWithReport();
            return null;
        }
        inputsSupplied++;
        return input;
    }

    /// <summary>Lets the session compare the frame the host just ran; its first divergence ends the movie.</summary>
    public void FrameCompleted(FrontendFrame frame)
    {
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
