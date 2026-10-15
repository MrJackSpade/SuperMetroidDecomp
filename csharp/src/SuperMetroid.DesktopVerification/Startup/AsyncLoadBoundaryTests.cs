using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Desktop.Window;

namespace SuperMetroid.DesktopVerification.Startup;

/// <summary>Exercises a load or restart while presentation holds the host's frame publication gate.</summary>
internal static class AsyncLoadBoundaryTests
{
    /// <summary>Ensures old game state survives until presentation exits and stale packets cannot publish after the load.</summary>
    internal static async Task VerifyAsyncLoadBoundary(PlayableGameControl control, bool restart = false)
    {
        var worker = control.gpuWorker ?? throw new InvalidOperationException("Host has no GPU worker.");
        var gate = worker.gate;
        var oldGame = control.game ?? throw new InvalidOperationException("Host has no game.");
        var packet = control.pendingDisplay ?? throw new InvalidOperationException("Host has no pending display.");
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task held = Task.Run(() => gate.TryPresent(packet.Identity, () =>
        {
            entered.Set();
            // Unbounded: under load the host can take longer than any fixed bound to begin
            // the load, and a deadlock still shows as a hang rather than a timed-out pass.
            release.Wait();
        }));
        Task? load = null;
        try
        {
            entered.Wait();
            load = restart ? control.RestartAsync() : control.LoadDebuggerState(0);
            Check(!load.IsCompleted, "load must await an entered presentation");
            // A continuation on the WinForms synchronization context must execute
            // while the boundary is held; synchronously waiting here deadlocks it.
            await Task.Delay(25);
            Check(ReferenceEquals(oldGame, control.game), "restore occurred before old Present finished");
            Check(!control.Enabled, "frame-affecting controls remained enabled during load");
        }
        finally
        {
            release.Set();
            await held;
            if (load is not null) await load;
        }
        Check(control.Enabled, "load did not restore host controls");
        Check(!gate.TryPresent(packet.Identity, () => throw new InvalidOperationException("stale frame")),
            "old generation still presents after async load");
    }
}
