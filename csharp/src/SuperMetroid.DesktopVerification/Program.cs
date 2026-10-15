using SuperMetroid.Core.Rendering;
using static SuperMetroid.DesktopVerification.Audio.AudioEndpointRecoveryTests;
using static SuperMetroid.DesktopVerification.Audio.AudioQueueHealthTests;
using static SuperMetroid.DesktopVerification.Startup.CloseDuringLoadTests;
using static SuperMetroid.DesktopVerification.Soak.DesktopTimerSoakTests;
using static SuperMetroid.DesktopVerification.Startup.FormCloseTests;
using static SuperMetroid.DesktopVerification.Soak.HiddenSoakTests;
using static SuperMetroid.DesktopVerification.Window.RendererCaptureTests;
using static SuperMetroid.DesktopVerification.Startup.RendererStartupTests;
using SuperMetroid.DesktopVerification.Audio;
using SuperMetroid.DesktopVerification.Harness;
using SuperMetroid.DesktopVerification.Soak;
using SuperMetroid.DesktopVerification.Startup;
namespace SuperMetroid.DesktopVerification;
using SuperMetroid.TestSupport.WindowsConsole.Verification;
using SuperMetroid.TestSupport.WindowsConsole;

/// <summary>Hidden-HWND integration checks: no visible window, native dialog, or player save mutation.</summary>
internal static class Program
{
    /// <summary>
    /// The default run's checks. Each uses its own scratch installation and hidden window, so the
    /// no-argument run gives each its own child process and runs them at the same time.
    /// </summary>
    private static readonly (string Name, Func<Task> Run)[] defaultChecks =
    [
        ("software-capture", () => Verify(RendererSelection.Software)),
        ("direct3d11-capture", () => Verify(RendererSelection.Direct3D11)),
        ("auto-capture", () => Verify(RendererSelection.Auto)),
        ("software-form-close", () => VerifyFormClose(RendererSelection.Software, duringStartup: false)),
        ("direct3d11-form-close", () => VerifyFormClose(RendererSelection.Direct3D11, duringStartup: false)),
        ("direct3d11-startup-form-close", () => VerifyFormClose(RendererSelection.Direct3D11, duringStartup: true)),
        ("close-during-load", () => VerifyCloseDuringLoad(restart: false)),
        ("close-during-load-restart", () => VerifyCloseDuringLoad(restart: true)),
        ("auto-startup-failure", () => VerifyRendererStartupFailure(RendererSelection.Auto)),
        ("direct3d11-startup-failure", () => VerifyRendererStartupFailure(RendererSelection.Direct3D11)),
        ("host-audio-acknowledgements", () => HostAudioAcknowledgementTests.Verify()),
    ];

    /// <summary>Captures initialization and message-loop failures at the console process boundary.</summary>
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

    /// <summary>Starts hidden desktop fixture commands with dialog suppression and a guarded idle callback.</summary>
    private static void Run(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        // Child fixture for --console-startup-boundary: it must fail at the console boundary.
        if (args is [ConsoleStartupBoundaryTests.FailingChildFlag])
        {
            NativeConsoleErrorPolicyCheck.VerifyDialogsDisabled();
            throw new InvalidOperationException(ConsoleStartupBoundaryTests.ExpectedFailure);
        }
        if (args is ["--console-startup-boundary"])
        {
            ConsoleStartupBoundaryTests.Run();
            return;
        }
        if (args.Length == 0)
        {
            DesktopCheckProcesses.RunAll(defaultChecks.Select(check => check.Name));
            Console.WriteLine("Desktop capture: software/GPU stepping, resize, paused state restore, generation reset and asynchronous shutdown passed.");
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
                if (args is [DesktopCheckProcesses.CheckFlag, var checkName])
                {
                    foreach (var check in defaultChecks)
                        if (check.Name == checkName)
                        {
                            await check.Run();
                            return;
                        }
                    throw new ArgumentException($"Unknown desktop check '{checkName}'.");
                }
                throw new ArgumentException("Usage: DesktopVerification [--audio-queue | --check NAME]");
            }
            catch (Exception error) { Console.Error.WriteLine(error); Environment.ExitCode = 1; }
            finally { context.ExitThread(); }
        };
        Application.Run(context);
    }
}
