using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Rendering.Direct3D11.Presentation;
using static SuperMetroid.DesktopVerification.Startup.AsyncLoadBoundaryTests;
using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
namespace SuperMetroid.DesktopVerification.Window;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Hidden-HWND renderer capture: retained display across debugger load, restart and resize.</summary>
internal static class RendererCaptureTests
{
    /// <summary>Direct3D 11 stepping, resize, paused state restore, generation reset and asynchronous shutdown.</summary>
    [ReflectionAudit("hidden forms receive no native resize message, so WinForms OnResize is raised directly")]
    internal static async Task Verify()
    {
        // Private fixture data lives in a new directory, never in the player's live slots.
        using var directoryScratch = new TestScratchDirectory("desktop-renderer");
        string directory = directoryScratch.Root;
        Directory.CreateDirectory(directory);
        string rom = PlaceInstalledRom(directory);
        using var form = new Form { ClientSize = new(900, 760), ShowInTaskbar = false };
        using var control = DesktopAccess.CreateGameControl(rom, new HostOptions { AudioEnabled = false });
        form.Controls.Add(control);
        // Construct handles without Show/ShowDialog; the UI loop remains alive for DXGI.
        _ = form.Handle;
        _ = control.Handle;
        control.SetPlaying(false);
        try
        {
            await control.InitializeRendererAsync();
            for (int i = 0; i < 30; i++) control.StepFrame(0);
            var before = control.pendingDisplay ?? throw new InvalidOperationException("Host has no pending display.");
            var game = control.game ?? throw new InvalidOperationException("Host has no game.");
            int frameNumber = game.FrameNumber;
            var expected = DisplayRendering.Render(before);
            control.SaveDebuggerState(0);
            for (int i = 0; i < 10; i++) control.StepFrame(0);
            await VerifyAsyncLoadBoundary(control);
            var restored = control.pendingDisplay ?? throw new InvalidOperationException("Host has no pending display.");
            Check(restored.Identity.Generation > before.Identity.Generation, "load must advance host generation");
            Check(restored.Identity.Sequence > before.Identity.Sequence, "load must not rewind host sequence");
            Check(control.game.FrameNumber == frameNumber, "load must not step game");
            Check(expected.SequenceEqual(DisplayRendering.Render(restored)), "loaded display must match saved pixels");
            form.ClientSize = new(700, 500);
            var worker = control.gpuWorker ?? throw new InvalidOperationException("Host has no GPU worker.");
            await Until(() => worker.LastConsumedSequence == restored.Identity.Sequence, worker);
            Check(worker.mailbox.Generation == restored.Identity.Generation,
                "worker must use loaded generation");
            form.WindowState = FormWindowState.Minimized;
            // Hidden forms do not receive native minimize messages. Raise the
            // real host event explicitly while retaining their child dimensions.
            (typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Verification requires typeof(Form).GetMethod(\"OnResize\", BindingFlags.Instance | BindingFlags.NonPublic).")).Invoke(form, [EventArgs.Empty]);
            await Until(() => worker.IsSurfaceSuspended, worker);
            form.WindowState = FormWindowState.Normal;
            (typeof(Form).GetMethod("OnResize", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Verification requires typeof(Form).GetMethod(\"OnResize\", BindingFlags.Instance | BindingFlags.NonPublic).")).Invoke(form, [EventArgs.Empty]);
            await Until(() => !worker.IsSurfaceSuspended, worker);
            var canvas = control.canvas;
            await Until(() => worker.LastDrawnSize
                == DisplayViewport.ClientSize(canvas.ClientSize.Width, canvas.ClientSize.Height), worker);
            await VerifyAsyncLoadBoundary(control, restart: true);
            var reset = control.pendingDisplay ?? throw new InvalidOperationException("Host has no pending display.");
            Check(reset.Identity.Generation > restored.Identity.Generation, "restart must advance generation");
            await Until(() => worker.LastConsumedSequence == reset.Identity.Sequence, worker);
        }
        finally { await control.StopRendererAsync(); }
        control.Dispose();
        form.Dispose();
        // Only this test's freshly allocated GUID directory is removed, after all owned
        // recorder streams and HWNDs close. Failures retain their fixture for diagnosis.
        Directory.Delete(directory, recursive: true);
        Console.WriteLine("  Direct3D11: retained load/reset display verified; test-owned files removed.");
    }
}
