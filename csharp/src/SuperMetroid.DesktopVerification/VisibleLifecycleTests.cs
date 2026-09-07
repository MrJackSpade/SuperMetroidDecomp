using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    /// <summary>Opt-in native visible-window lifecycle test; never touches player saves.</summary>
    private static async Task VerifyVisibleMinimizeRestore()
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var form = new Form { Text = "Renderer minimize/restore verification", ClientSize = new(900, 760), ShowInTaskbar = false };
        using var control = new PlayableGameControl(rom, new SuperMetroidGameOptions { Renderer = RendererSelection.Direct3D11, AudioEnabled = false });
        form.Controls.Add(control);
        _ = form.Handle;
        _ = control.Handle;
        Call(control, "SetPlaying", false);
        try
        {
            await control.InitializeRendererAsync();
            var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
            bool Suspended() => (bool)typeof(D3D11RenderWorker).GetProperty("IsSurfaceSuspended", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(worker)!;
            VisibleSoakWindow.ShowWithoutActivation(form.Handle);
            Call(control, "StepFrame", (ushort)0);
            await Until(() => worker.PresentedFrames > 0, worker);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                // Actual visible HWND messages, without manually raising OnResize.
                form.WindowState = FormWindowState.Minimized;
                await Until(Suspended, worker);
                long presented = worker.PresentedFrames;
                for (int frame = 0; frame < 12; frame++) Call(control, "StepFrame", (ushort)0);
                await Task.Delay(250);
                Check(worker.PresentedFrames == presented, "minimized window kept presenting");
                VisibleSoakWindow.ShowWithoutActivation(form.Handle);
                await Until(() => !Suspended(), worker);
                await Until(() => worker.PresentedFrames > presented, worker);
                var canvas = Field<RuntimeCanvas>(control, "canvas");
                await Until(() => ((int Width, int Height))typeof(D3D11RenderWorker).GetProperty("LastDrawnSize", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(worker)! == (canvas.ClientSize.Width, canvas.ClientSize.Height), worker);
                Console.WriteLine($"Visible minimize/restore cycle {cycle + 1}: native suspension, publication while minimized, restored presentation and dimensions passed.");
            }
        }
        finally { await control.StopRendererAsync(); }
        control.Dispose();
        form.Dispose();
        Directory.Delete(directory, recursive: true);
    }
}
