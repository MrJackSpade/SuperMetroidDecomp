using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Game;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task VerifyCloseDuringLoad(bool restart)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var form = new GameForm(rom, new SuperMetroidGameOptions { Renderer = RendererSelection.Direct3D11, AudioEnabled = false });
        _ = form.Handle;
        var control = Field<PlayableGameControl>(form, "gameControl");
        _ = control.Handle;
        Call(control, "SetPlaying", false);
        await control.InitializeRendererAsync();
        Call(control, "StepFrame", (ushort)0);
        Call(control, "SaveDebuggerState", 0);
        var oldGame = Field<SuperMetroidGame>(control, "game");
        var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
        var gate = Field<RenderPresentationGate>(worker, "gate");
        var packet = Field<RenderFrameSnapshot>(control, "pendingDisplay");
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Task held = Task.Run(() => gate.TryPresent(packet.Identity, () =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Closing load release missing.");
        }));
        Task? load = null;
        try
        {
            if (!entered.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Closing load boundary was not acquired.");
            load = restart ? CallAsync(control, "RestartAsync") : CallAsync(control, "LoadDebuggerState", 0);
            Check(!load.IsCompleted, "closing-load fixture did not hold the pending operation");
            form.Close();
            Check(Field<bool>(control, "rendererStopping"), "close did not initiate renderer shutdown");
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
        Check(ReferenceEquals(oldGame, Field<SuperMetroidGame>(control, "game")), "closing load replaced the game after shutdown began");
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"  Close during pending {(restart ? "restart" : "load")}: no late restore or shutdown fault.");
    }
}
