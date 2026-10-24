using SuperMetroid.Diagnostics.Configuration;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Desktop.Window;
namespace SuperMetroid.DesktopVerification.Startup;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks desktop shutdown while debugger restore or restart waits on GPU presentation.</summary>
internal static class CloseDuringLoadTests
{
    /// <summary>Holds a renderer presentation gate, closes the form during the pending load, and confirms no late game replacement or GPU worker fault.</summary>
    internal static async Task VerifyCloseDuringLoad(bool restart)
    {
        using var directoryScratch = new TestScratchDirectory("desktop-renderer");
        string directory = directoryScratch.Root;
        Directory.CreateDirectory(directory);
        string rom = PlaceInstalledRom(directory);
        using var form = new GameForm(DesktopAccess.InstallationFor(rom), new HostOptions { AudioEnabled = false });
        _ = form.Handle;
        var control = form.gameControl;
        _ = control.Handle;
        control.SetPlaying(false);
        await control.InitializeRendererAsync();
        control.StepFrame(0);
        control.SaveDebuggerState(0);
        var oldGame = control.game ?? throw new InvalidOperationException("Host has no game.");
        var worker = control.gpuWorker ?? throw new InvalidOperationException("Host has no GPU worker.");
        var gate = worker.gate;
        var packet = control.pendingDisplay ?? throw new InvalidOperationException("Host has no pending display.");
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task held = Task.Run(() => gate.TryPresent(packet.Identity, () =>
        {
            entered.Set();
            // Unbounded, as in the async load boundary check: a fixed bound fails healthy loads under contention.
            release.Wait();
        }));
        Task? load = null;
        try
        {
            entered.Wait();
            load = restart ? control.RestartAsync() : control.LoadDebuggerState(0);
            Check(!load.IsCompleted, "closing-load fixture did not hold the pending operation");
            form.Close();
            Check(control.rendererStopping, "close did not initiate renderer shutdown");
        }
        finally
        {
            release.Set();
            await held;
            try { if (load is not null) await load; }
            finally { if (!control.IsDisposed) await control.StopRendererAsync(); }
        }
        long deadline = Environment.TickCount64 + 10000;
        while (!form.IsDisposed)
        {
            if (Environment.TickCount64 >= deadline) throw new TimeoutException("Close during load did not finish.");
            await Task.Delay(10);
        }
        Check(worker.Completion.IsCompletedSuccessfully, "close during load faulted the GPU owner");
        Check(ReferenceEquals(oldGame, control.game), "closing load replaced the game after shutdown began");
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"  Close during pending {(restart ? "restart" : "load")}: no late restore or shutdown fault.");
    }
}
