using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task VerifyRendererStartupFailure(RendererSelection selection)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var control = new PlayableGameControl(rom, new SuperMetroidGameOptions { Renderer = selection, AudioEnabled = false });
        _ = control.Handle;
        Call(control, "SetPlaying", false);
        var game = Field<SuperMetroidGame>(control, "game");
        ushort frame = game.FrameNumber;
        D3D11RenderWorker? failedWorker = null;
        using var diagnostic = new StringWriter();
        TextWriter previousError = Console.Error;
        Exception? failure = null;
        try
        {
            Console.SetError(diagnostic);
            try
            {
                await control.InitializeRendererAsync((_, width, height, generation) =>
                    failedWorker = new D3D11RenderWorker(0, width, height, generation, D3D11DeviceKind.Hardware));
            }
            catch (Exception error) { failure = error; }
            Check(failedWorker is { Completion.IsFaulted: true }, "failed startup worker was not fully drained");
            Check(game.FrameNumber == frame, "startup failure advanced the game");
            Check(!Field<bool>(Field<RuntimeCanvas>(control, "canvas"), "gpuOwned"), "failed startup left GDI disabled");
            if (selection == RendererSelection.Auto)
            {
                Check(failure is null && diagnostic.ToString().Contains("Auto selected software rendering"), "Auto fallback was not explicit and logged");
                Call(control, "StepFrame", (ushort)0);
                Check(game.FrameNumber == unchecked((ushort)(frame + 1)), "software fallback could not continue stepping");
            }
            else Check(failure is ArgumentException, "explicit Direct3D11 swallowed startup failure");
        }
        finally
        {
            Console.SetError(previousError);
            await control.StopRendererAsync();
        }
        bool restartRejected = false;
        try { await control.InitializeRendererAsync(); } catch (InvalidOperationException) { restartRejected = true; }
        Check(restartRejected, "stopped renderer accepted new startup");
        control.Dispose();
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"Renderer startup {selection}: real asynchronous failure, logged fallback/explicit error and stopped-lifecycle guard passed.");
    }
}
