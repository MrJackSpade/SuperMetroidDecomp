using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Game;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task VerifyFormClose(RendererSelection renderer, bool duringStartup)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var form = new GameForm(rom, new SuperMetroidGameOptions { Renderer = renderer, AudioEnabled = false });
        _ = form.Handle;
        var control = Field<PlayableGameControl>(form, "gameControl");
        _ = control.Handle;
        Call(control, "SetPlaying", false);
        Task startup = control.InitializeRendererAsync();
        D3D11RenderWorker? worker = renderer == RendererSelection.Direct3D11
            ? Field<D3D11RenderWorker>(control, "gpuWorker") : null;
        try
        {
            if (!duringStartup) await startup;
            bool destroyedBeforeWorkerStopped = false;
            control.HandleDestroyed += (_, _) => destroyedBeforeWorkerStopped |= worker is { Completion.IsCompleted: false };
            form.Close();
            long deadline = Environment.TickCount64 + 10000;
            while (!form.IsDisposed)
            {
                if (Environment.TickCount64 >= deadline) throw new TimeoutException("GameForm did not finish asynchronous close.");
                await Task.Delay(10);
            }
            await startup;
            Check(!destroyedBeforeWorkerStopped, "canvas HWND destroyed before GPU owner stopped");
            Check(worker is null || worker.Completion.IsCompletedSuccessfully, "form close did not join GPU owner");
            Check(Field<bool>(form, "rendererStopped"), "form did not complete its renderer shutdown path");
        }
        finally
        {
            if (!control.IsDisposed) await control.StopRendererAsync();
        }
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"  GameForm close: {renderer}, during startup={duringStartup}; HWND outlives worker.");
    }
}
