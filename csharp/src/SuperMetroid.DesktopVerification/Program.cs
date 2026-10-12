using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;
using static AsyncLoadBoundaryTests;
using static AudioEndpointRecoveryTests;
using static AudioQueueHealthTests;
using static CloseDuringLoadTests;
using static DesktopTimerSoakTests;
using static FormCloseTests;
using static HiddenSoakTests;
using static RendererCaptureTests;
using static RendererStartupTests;

/// <summary>Hidden-HWND integration checks: no visible window, native dialog, or player save mutation.</summary>
internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            Run(args);
        }
        catch (Exception error)
        {
            // Initialization, message-loop and disposal failures need the same
            // process boundary as the already-guarded asynchronous audit body.
            Console.Error.WriteLine(error);
            Environment.ExitCode = 1;
        }
    }

    private static void Run(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        // Child fixture for --console-startup-boundary: it must fail at the console boundary.
        if (args is [ConsoleStartupBoundaryTests.FailingChildFlag])
        {
            NativeConsoleErrors.VerifyDialogsDisabled();
            throw new InvalidOperationException(ConsoleStartupBoundaryTests.ExpectedFailure);
        }
        if (args is ["--console-startup-boundary"])
        {
            ConsoleStartupBoundaryTests.Run();
            return;
        }
        if (!Application.SetHighDpiMode(HighDpiMode.PerMonitorV2))
            throw new InvalidOperationException("Could not initialize desktop verification with the game's PerMonitorV2 DPI policy.");
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        using var context = new ApplicationContext();
        bool started = false;
        Application.Idle += async (_, _) =>
        {
            if (started) return;
            started = true;
            try
            {
                if (DesktopSmokeAuditCommands.TryRun(args)) return;
                VerifyAudioQueueHealth();
                if (args is ["--audio-endpoint-recovery-audit"])
                {
                    VerifyAudioEndpointRecovery();
                    return;
                }
                if (args is ["--audio-backpressure-audit"])
                {
                    VerifyAudioQueueBackpressure();
                    return;
                }
                if (args is ["--soak-desktop-hidden"])
                {
                    await RunDesktopTimerSoak(SoakDefinitions.SecondsPerScene);
                    return;
                }
                if (args is ["--soak-desktop-hidden-smoke"])
                {
                    await RunDesktopTimerSoak(SoakDefinitions.PublishSmokeSecondsPerScene);
                    return;
                }
                if (args is ["--soak-hidden"])
                {
                    await RunHiddenSoak(SoakDefinitions.SecondsPerScene);
                    return;
                }
                if (args is ["--audio-queue"])
                {
                    await VerifyNativeAudioQueueHealth();
                    return;
                }
                if (args.Length != 0) throw new ArgumentException("Usage: DesktopVerification [--audio-queue]");
                await Verify(RendererSelection.Software);
                await Verify(RendererSelection.Direct3D11);
                await Verify(RendererSelection.Auto);
                await VerifyFormClose(RendererSelection.Software, duringStartup: false);
                await VerifyFormClose(RendererSelection.Direct3D11, duringStartup: false);
                await VerifyFormClose(RendererSelection.Direct3D11, duringStartup: true);
                await VerifyCloseDuringLoad(restart: false);
                await VerifyCloseDuringLoad(restart: true);
                await VerifyRendererStartupFailure(RendererSelection.Auto);
                await VerifyRendererStartupFailure(RendererSelection.Direct3D11);
                Console.WriteLine("Desktop capture: software/GPU stepping, resize, paused state restore, generation reset and asynchronous shutdown passed.");
            }
            catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
            finally { context.ExitThread(); }
        };
        Application.Run(context);
    }
}
