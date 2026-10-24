using SuperMetroid.Diagnostics.Configuration;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.DesktopVerification.Startup;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks desktop behavior after an asynchronous D3D11 renderer startup failure.</summary>
internal static class RendererStartupTests
{
    /// <summary>
    /// Confirms a Direct3D 11 startup failure surfaces as an error carrying the device's failure, never advances
    /// gameplay, and leaves a stopped renderer that refuses a restart.
    /// </summary>
    internal static async Task VerifyRendererStartupFailure()
    {
        using var directoryScratch = new TestScratchDirectory("desktop-renderer");
        string directory = directoryScratch.Root;
        Directory.CreateDirectory(directory);
        string rom = PlaceInstalledRom(directory);
        using var control = DesktopAccess.CreateGameControl(rom, new HostOptions { AudioEnabled = false });
        _ = control.Handle;
        control.SetPlaying(false);
        var game = control.game;
        int frame = game.FrameNumber;
        D3D11RenderWorker? failedWorker = null;
        Exception? failure = null;
        try
        {
            try
            {
                await control.InitializeRendererAsync((_, clientSize, generation) =>
                    failedWorker = new D3D11RenderWorker(0, clientSize, generation, D3D11DeviceKind.Hardware));
            }
            catch (Exception error) { failure = error; }
            Check(failedWorker is { Completion.IsFaulted: true }, "failed startup worker was not fully drained");
            Check(game.FrameNumber == frame, "startup failure advanced the game");
            Check(!control.canvas.gpuOwned, "failed startup left GDI disabled");
            Check(failure is InvalidOperationException { InnerException: ArgumentException },
                "Direct3D 11 startup failure was swallowed or lost the device's error");
        }
        finally
        {
            await control.StopRendererAsync();
        }
        bool restartRejected = false;
        try { await control.InitializeRendererAsync(); } catch (InvalidOperationException) { restartRejected = true; }
        Check(restartRejected, "stopped renderer accepted new startup");
        control.Dispose();
        Directory.Delete(directory, recursive: true);
        Console.WriteLine("Renderer startup: real asynchronous Direct3D 11 failure surfaced as an error and the stopped-lifecycle guard passed.");
    }
}
