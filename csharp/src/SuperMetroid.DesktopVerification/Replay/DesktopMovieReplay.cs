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

    /// <summary>The movie a command-line name selects.</summary>
    /// <param name="name">One of <c>glitch</c>, <c>13</c>, <c>100</c>, <c>drunk</c>, <c>lowc</c> or <c>ridley</c>.</param>
    internal static ReplayMovieId Parse(string name) => name switch
    {
        "glitch" => ReplayMovieId.MotherBrainGlitch,
        "13" => ReplayMovieId.LowPercent13,
        "100" => ReplayMovieId.Full100,
        "drunk" => ReplayMovieId.Drunk,
        "lowc" => ReplayMovieId.Lowc,
        "ridley" => ReplayMovieId.Ridley,
        _ => throw new ArgumentException(
            $"Unknown movie '{name}'; use glitch, 13, 100, drunk, lowc or ridley.", nameof(name)),
    };

    /// <summary>
    /// Runs the movie to its end through the host and prints its final report. The first divergence, gameplay
    /// failure or renderer fault is thrown to the process boundary. The movie runs unpaced (#1511); the verifier's
    /// 30-minute run limit is the only bound.
    /// </summary>
    internal static async Task Run(ReplayMovieId movie)
    {
        // The host writes player data and debugger slots into its installation root, so the movie runs on a
        // private copy of the repository installation.
        using var copy = InstalledFixtures.CreatePrivateCopy();
        using var session = ConvertedMovieSession.Open(movie);
        var driver = new MovieFrameDriver(session);
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
            // Only the production WinForms timer advances the movie from here.
            control.SetPlaying(true);
            Task ended = await Task.WhenAny(driver.Completion, worker.Completion);
            worker.ThrowIfFaulted();
            if (ended != driver.Completion)
                throw new InvalidOperationException("The Direct3D 11 worker stopped before the movie ended.");
            Console.WriteLine(await driver.Completion);
        }
        finally
        {
            await control.StopRendererAsync();
        }
    }
}
