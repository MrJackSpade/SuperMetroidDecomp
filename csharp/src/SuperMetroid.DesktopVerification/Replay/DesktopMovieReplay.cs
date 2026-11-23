using SuperMetroid.Rendering.Direct3D11.Telemetry;
using SuperMetroid.Desktop.Window;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.DesktopVerification.Replay;

/// <summary>
/// Replays one supplied SMV movie through the production Direct3D 11 host: a hidden <see cref="GameForm"/>
/// whose own WinForms timer advances every frame, with presentation on the hardware Direct3D 11 worker.
/// </summary>
internal static class DesktopMovieReplay
{
    /// <summary>The command-line flag naming the movie to replay.</summary>
    internal const string Flag = "--desktop-movie";

    /// <summary>
    /// The flag that stops the movie after a number of updates and reports each update the host took long over (#2619):
    /// where a player saw the game stall.
    /// </summary>
    internal const string UntilFlag = "--until";

    /// <summary>The flag that measures every frame's update, render and present and writes them to a file (#2653).</summary>
    internal const string FrameTimesFlag = "--frame-times";

    /// <summary>One frame at 60 Hz: a frame slower than this misses its slot.</summary>
    private const double FrameBudgetMilliseconds = 1000.0 / 60;

    /// <summary>The movie a command-line name selects.</summary>
    /// <param name="name">One of <c>glitch</c>, <c>13</c>, <c>100</c>, <c>lowc</c> or <c>ridley</c>.</param>
    internal static ReplayMovieId Parse(string name) => name switch
    {
        "glitch" => ReplayMovieId.MotherBrainGlitch,
        "13" => ReplayMovieId.LowPercent13,
        "100" => ReplayMovieId.Full100,
        "lowc" => ReplayMovieId.Lowc,
        "ridley" => ReplayMovieId.Ridley,
        _ => throw new ArgumentException(
            $"Unknown movie '{name}'; use glitch, 13, 100, lowc or ridley.", nameof(name)),
    };

    /// <summary>
    /// Runs the movie to its end through the host and prints its final report. The first divergence, gameplay
    /// failure or renderer fault is thrown to the process boundary. The movie runs unpaced (#1511); the verifier's
    /// 30-minute run limit is the only bound.
    /// </summary>
    /// <param name="movie">The movie.</param>
    /// <param name="until">The updates to run before stopping, reporting each slow update, or null for the whole movie.</param>
    /// <param name="frameTimes">The file every frame's timing is written to, for a measured replay; null for none.</param>
    internal static async Task Run(ReplayMovieId movie, int? until, string? frameTimes = null)
    {
        // The host writes player data and debugger slots into its installation root, so the movie runs on a
        // private copy of the repository installation.
        using var copy = InstalledFixtures.CreatePrivateCopy();
        using var session = ConvertedMovieSession.Open(movie);
        var driver = new MovieFrameDriver(session, until, measure: frameTimes is not null);
        using var form = new GameForm(copy.Installation, new HostOptions { AudioEnabled = false }, driver);
        var control = form.gameControl;
        _ = form.Handle;
        _ = control.Handle;
        // The host starts playing on construction; hold the timer until the renderer presents.
        control.SetPlaying(false);
        try
        {
            await control.InitializeRendererAsync();
            var worker = control.gpuWorker ?? throw new InvalidOperationException("The host has no Direct3D 11 worker.");
            if (frameTimes is not null)
            {
                driver.DisplaySequence = () => control.DisplaySequence;
                worker.StartFrameLog();
            }
            // Only the production WinForms timer advances the movie from here.
            control.SetPlaying(true);
            Task ended = await Task.WhenAny(driver.Completion, worker.Completion);
            worker.ThrowIfFaulted();
            if (ended != driver.Completion)
                throw new InvalidOperationException("The Direct3D 11 worker stopped before the movie ended.");
            Console.WriteLine(await driver.Completion);
            if (frameTimes is not null)
                WriteFrameTimes(frameTimes, driver.MeasuredUpdates, worker.FrameLog());
        }
        finally
        {
            await control.StopRendererAsync();
        }
    }

    /// <summary>
    /// Writes every update's update, render and present times to <paramref name="path"/>, one line each, and reports the
    /// frames slower than one 60 Hz frame (#2653).
    /// </summary>
    /// <exception cref="InvalidDataException">An update's frame was never rendered, or rendered twice.</exception>
    private static void WriteFrameTimes(string path, IReadOnlyList<MeasuredUpdate> updates, IReadOnlyList<RenderedFrameTiming> rendered)
    {
        var bySequence = new Dictionary<long, RenderedFrameTiming>();
        foreach (RenderedFrameTiming timing in rendered)
        {
            if (!bySequence.TryAdd(timing.Sequence, timing))
                throw new InvalidDataException($"The render worker submitted frame {timing.Sequence} twice.");
        }
        int slow = 0;
        using var writer = new StreamWriter(path);
        writer.WriteLine("update\tsource\tphase\troom\tstate\tupdate_ms\trender_ms\tpresent_ms\ttotal_ms");
        foreach (MeasuredUpdate update in updates)
        {
            RenderedFrameTiming timing = bySequence.TryGetValue(update.Sequence, out RenderedFrameTiming found) ? found
                : throw new InvalidDataException($"Update {update.Update}'s frame {update.Sequence} was never rendered.");
            double total = update.UpdateMilliseconds + timing.CompositionMilliseconds + timing.PresentMilliseconds;
            if (total > FrameBudgetMilliseconds)
                slow++;
            writer.WriteLine(string.Join('\t', update.Update, update.SourceFrame, update.Phase, update.Room, update.RoomState,
                update.UpdateMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                timing.CompositionMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                timing.PresentMilliseconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                total.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)));
        }
        Console.WriteLine($"Measured {updates.Count} frames; {slow} slower than {FrameBudgetMilliseconds:F1} ms; written to {path}.");
    }
}
