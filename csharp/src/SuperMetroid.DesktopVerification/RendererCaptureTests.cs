using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;
using static AsyncLoadBoundaryTests;
using static DesktopVerificationSupport;

/// <summary>Hidden-HWND renderer capture: retained display across debugger load, restart and resize.</summary>
internal static class RendererCaptureTests
{
    /// <summary>Software/GPU stepping, resize, paused state restore, generation reset and asynchronous shutdown for one renderer.</summary>
    internal static async Task Verify(RendererSelection renderer)
    {
        // Private fixture data lives in a new directory, never in the player's live slots.
        using var directoryScratch = new TestScratchDirectory("desktop-renderer");
        string directory = directoryScratch.Root;
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
                    == DisplayViewport.ClientSize(canvas.ClientSize.Width, canvas.ClientSize.Height), worker);
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
}
