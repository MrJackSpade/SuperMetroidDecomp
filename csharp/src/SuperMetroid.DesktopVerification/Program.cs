using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Hidden-HWND integration checks: no visible window, native dialog, or player save mutation.</summary>
internal static partial class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
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
                VerifyAudioQueueHealth();
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
                if (args is ["--visible-minimize-restore"])
                {
                    await VerifyVisibleMinimizeRestore();
                    return;
                }
                if (args is ["--soak-desktop-visible", var visibleDuration] && int.TryParse(visibleDuration, out int visibleSeconds))
                {
                    await RunDesktopTimerSoak(visibleSeconds, visible: true);
                    return;
                }
                if (args is ["--soak-desktop-hidden", var desktopDuration] && int.TryParse(desktopDuration, out int desktopSeconds))
                {
                    await RunDesktopTimerSoak(desktopSeconds);
                    return;
                }
                if (args is ["--soak-hidden", var durationText] && int.TryParse(durationText, out int seconds))
                {
                    await RunHiddenSoak(seconds);
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

    private static async Task Verify(RendererSelection renderer)
    {
        // Private fixture data lives in a new directory, never in the player's live slots.
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        using var form = new Form { ClientSize = new(900, 760), ShowInTaskbar = false };
        using var control = new PlayableGameControl(rom, new SuperMetroidGameOptions { Renderer = renderer, AudioEnabled = false });
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
                Check(worker.MailboxMetrics.Generation == restored.Identity.Generation, "worker must use loaded generation");
                form.WindowState = FormWindowState.Minimized;
                // Hidden forms do not receive native minimize messages. Raise the
                // real host event explicitly while retaining their child dimensions.
                typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [EventArgs.Empty]);
                await Until(() => (bool)typeof(D3D11RenderWorker).GetProperty("IsSurfaceSuspended",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(worker)!, worker);
                form.WindowState = FormWindowState.Normal;
                typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [EventArgs.Empty]);
                await Until(() => !(bool)typeof(D3D11RenderWorker).GetProperty("IsSurfaceSuspended",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(worker)!, worker);
                var canvas = Field<RuntimeCanvas>(control, "canvas");
                await Until(() => ((int Width, int Height))typeof(D3D11RenderWorker)
                    .GetProperty("LastDrawnSize", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(worker)!
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
    private static void Call(object owner, string name, params object[] args) =>
        (owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args);
    private static Task CallAsync(object owner, string name, params object[] args) =>
        (Task)((owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args)
            ?? throw new InvalidOperationException("Async handler returned no task."));
    private static T Field<T>(object owner, string name) =>
        (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)
            ?? throw new MissingFieldException(name));
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
