using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Hidden-HWND integration checks: no visible window, native dialog, or player save mutation.</summary>
internal static partial class Program
{
    /// <summary>Starts the hidden-window desktop verification process and reports uncaught startup failures to stderr.</summary>
    /// <param name="args">Command-line options selecting the verification audit to run.</param>
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

    /// <summary>Configures the Windows Forms host and runs the requested desktop verification on its idle loop.</summary>
    /// <param name="args">Command-line options selecting an audit, soak, or the default renderer checks.</param>
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
                if (args is ["--space-jump-audio-audit"])
                {
                    VerifySpaceJumpAudio();
                    return;
                }
                if (args is ["--statue-entry-audit"])
                {
                    VerifyStatueEntry();
                    return;
                }
                if (args is ["--metal-pirates-audit"])
                {
                    VerifyMetalPirates();
                    return;
                }
                if (args is ["--enemy-knockback-reentry-audit"])
                {
                    VerifyEnemyKnockbackReentry();
                    return;
                }
                if (args is ["--draygon-grapple-audit"])
                {
                    VerifyDraygonGrapple();
                    return;
                }
                if (args is ["--draygon-position-audit"])
                {
                    VerifyDraygonPosition();
                    return;
                }
                if (args is ["--grapple-auto-retract-audit"])
                {
                    VerifyGrappleAutomaticRetraction();
                    return;
                }
                if (args is ["--grapple-retry-audit"])
                {
                    VerifyGrappleRetry();
                    return;
                }
                if (args is ["--stuck-grapple-audit"])
                {
                    VerifyStuckGrapple();
                    return;
                }
                if (args is ["--anchored-grapple-audit"])
                {
                    VerifyAnchoredGrapple();
                    return;
                }
                if (args is ["--shinespark-shaft-audit"])
                {
                    VerifyShinesparkShaft();
                    return;
                }
                if (args is ["--grapple-release-state-audit"])
                {
                    VerifyGrappleReleaseState();
                    return;
                }
                if (args is ["--chozo-state-audit"])
                {
                    VerifyChozoStatueState();
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
                DebuggerRetiredFieldTests.Run();
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

    /// <summary>Checks save/restore, display identity, resizing, and shutdown for a renderer in an isolated ROM copy.</summary>
    /// <param name="renderer">Renderer backend selected for this verification instance.</param>
    /// <returns>A task that completes after the form and renderer have been shut down and the fixture is removed.</returns>
    private static async Task Verify(RendererSelection renderer)
    {
        // Private fixture data lives in a new directory, never in the player's live slots.
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var form = new Form { ClientSize = new(900, 760), ShowInTaskbar = false };
        using var control = DesktopAccess.CreateGameControl(rom, new SuperMetroidGameOptions { Renderer = renderer, AudioEnabled = false });
        form.Controls.Add(control);
        // Construct handles without Show/ShowDialog; the UI loop remains alive for DXGI.
        _ = form.Handle;
        _ = control.Handle;
        Call(control, "SetPlaying", false);
        try
        {
            await control.InitializeRendererAsync();
            for (int i = 0; i < 30; i++) Call(control, "StepFrame", (ushort)0);
            var before = Field<RenderFrameSnapshot>(control, "pendingDisplay");
            var game = Field<SuperMetroidGame>(control, "game");
            ushort frameNumber = game.FrameNumber;
            var expected = SoftwareFrameSnapshotRenderer.Render(before);
            Call(control, "SaveDebuggerState", 0);
            for (int i = 0; i < 10; i++) Call(control, "StepFrame", (ushort)0);
            if (renderer != RendererSelection.Software)
                await VerifyAsyncLoadBoundary(control);
            else await CallAsync(control, "LoadDebuggerState", 0);
            var restored = Field<RenderFrameSnapshot>(control, "pendingDisplay");
            Check(restored.Identity.Generation > before.Identity.Generation, "load must advance host generation");
            Check(restored.Identity.Sequence > before.Identity.Sequence, "load must not rewind host sequence");
            Check(Field<SuperMetroidGame>(control, "game").FrameNumber == frameNumber, "load must not step game");
            Check(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(restored)), "loaded display must match saved pixels");
            form.ClientSize = new(700, 500);
            if (renderer != RendererSelection.Software)
            {
                var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
                await Until(() => worker.LastConsumedSequence == restored.Identity.Sequence, worker);
                Check(Field<LatestRenderFrameMailbox>(worker, "mailbox").Generation == restored.Identity.Generation,
                    "worker must use loaded generation");
                form.WindowState = FormWindowState.Minimized;
                // Hidden forms do not receive native minimize messages. Raise the
                // real host event explicitly while retaining their child dimensions.
                typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [EventArgs.Empty]);
                await Until(() => worker.IsSurfaceSuspended, worker);
                form.WindowState = FormWindowState.Normal;
                typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [EventArgs.Empty]);
                await Until(() => !worker.IsSurfaceSuspended, worker);
                var canvas = Field<RuntimeCanvas>(control, "canvas");
                await Until(() => worker.LastDrawnSize
                    == (canvas.ClientSize.Width, canvas.ClientSize.Height), worker);
            }
            if (renderer != RendererSelection.Software)
                await VerifyAsyncLoadBoundary(control, restart: true);
            else await CallAsync(control, "RestartAsync");
            var reset = Field<RenderFrameSnapshot>(control, "pendingDisplay");
            Check(reset.Identity.Generation > restored.Identity.Generation, "restart must advance generation");
            if (renderer != RendererSelection.Software)
            {
                var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
                await Until(() => worker.LastConsumedSequence == reset.Identity.Sequence, worker);
            }
        }
        finally { await control.StopRendererAsync(); }
        control.Dispose();
        form.Dispose();
        // Only this test's freshly allocated GUID directory is removed, after all owned
        // recorder streams and HWNDs close. Failures retain their fixture for diagnosis.
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"  {renderer}: retained load/reset display verified; test-owned files removed.");
    }

    /// <summary>Waits for a GPU-worker condition while surfacing worker faults and enforcing a ten-second deadline.</summary>
    /// <param name="condition">Predicate that signals the awaited worker state.</param>
    /// <param name="worker">Worker checked for faults while the predicate remains false.</param>
    /// <returns>A task that completes when the condition becomes true.</returns>
    /// <exception cref="TimeoutException">The condition remains false past the deadline.</exception>
    private static async Task Until(Func<bool> condition, D3D11RenderWorker worker)
    {
        long deadline = Environment.TickCount64 + 10000;
        while (!condition())
        {
            worker.ThrowIfFaulted();
            if (Environment.TickCount64 >= deadline) throw new TimeoutException("Desktop GPU did not consume latest display.");
            await Task.Delay(10);
        }
    }

    // Exercise the actual toolbar handlers without exposing debug-only production APIs.
    /// <summary>Invokes a named nonpublic instance method for the desktop-control fixture.</summary>
    /// <param name="owner">Object declaring the handler.</param>
    /// <param name="name">Handler method name to find and invoke.</param>
    /// <param name="args">Arguments forwarded to reflection invocation.</param>
    private static void Call(object owner, string name, params object[] args) =>
        (owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args);

    /// <summary>Invokes a named nonpublic instance handler and returns its task result.</summary>
    /// <param name="owner">Object declaring the handler.</param>
    /// <param name="name">Handler method name to find and invoke.</param>
    /// <param name="args">Arguments forwarded to reflection invocation.</param>
    /// <returns>The task returned by the handler.</returns>
    private static Task CallAsync(object owner, string name, params object[] args) =>
        (Task)((owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args)
            ?? throw new InvalidOperationException("Async handler returned no task."));

    /// <summary>Reads a required nonpublic instance field from a desktop-control fixture.</summary>
    /// <typeparam name="T">Expected field value type.</typeparam>
    /// <param name="owner">Object containing the field.</param>
    /// <param name="name">Field name to find and read.</param>
    /// <returns>The field value cast to <typeparamref name="T"/>.</returns>
    private static T Field<T>(object owner, string name) =>
        (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)
            ?? throw new MissingFieldException(name));

    /// <summary>Fails the current verification immediately when its expected condition is false.</summary>
    /// <param name="condition">Condition that must hold for the check to pass.</param>
    /// <param name="message">Failure description included in the thrown exception.</param>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
