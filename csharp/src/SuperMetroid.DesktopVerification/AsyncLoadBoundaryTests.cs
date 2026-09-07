using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task VerifyAsyncLoadBoundary(PlayableGameControl control, bool restart = false)
    {
        var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
        var gate = Field<RenderPresentationGate>(worker, "gate");
        var oldGame = Field<SuperMetroidGame>(control, "game");
        var packet = Field<RenderFrameSnapshot>(control, "pendingDisplay");
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task held = Task.Run(() => gate.TryPresent(packet.Identity, () =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Load boundary release missing.");
        }));
        Task? load = null;
        try
        {
            if (!entered.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Load boundary was not acquired.");
            load = restart ? CallAsync(control, "RestartAsync") : CallAsync(control, "LoadDebuggerState", 0);
            Check(!load.IsCompleted, "load must await an entered presentation");
            // A continuation on the WinForms synchronization context must execute
            // while the boundary is held; synchronously waiting here deadlocks it.
            await Task.Delay(25);
            Check(ReferenceEquals(oldGame, Field<SuperMetroidGame>(control, "game")), "restore occurred before old Present finished");
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
