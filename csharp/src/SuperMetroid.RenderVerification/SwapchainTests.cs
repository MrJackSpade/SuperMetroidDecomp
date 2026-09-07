using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Predefined STATIC class, no WS_VISIBLE, no activation or message dialog.
        nint window = CreateWindowExW(0, "STATIC", "Hidden renderer verification", 0,
            0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            using var presenter = new D3D11SwapchainPresenter(device, window, 640, 480);
            var gate = new RenderPresentationGate(1);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(173,91,27));
            renderer.Render(packet);
            _ = presenter.TryAcquireFrameOpportunity();
            var outcome = presenter.Present(renderer, packet.Identity, gate);
            if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Current frame was rejected.");
            gate.AdvanceGeneration(2);
            if (presenter.Present(renderer, packet.Identity, gate) != D3D11PresentationResult.StaleGeneration)
                throw new InvalidOperationException("Swapchain presented stale generation.");
            bool mismatch = false;
            try { presenter.Present(renderer, new(2,2,0), gate); }
            catch (InvalidOperationException) { mismatch = true; }
            if (!mismatch) throw new InvalidOperationException("Swapchain accepted mismatched GPU frame identity.");
            foreach (var size in new[] { (319,601), (1,1), (800,600) })
            {
                presenter.Resize(size.Item1, size.Item2);
                packet = new(new(packet.Identity.Sequence + 1, 2, 0), new Rgba32(27,91,173));
                renderer.Render(packet);
                outcome = presenter.Present(renderer, packet.Identity, gate);
                if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Resized frame was rejected.");
            }
            Console.WriteLine($"{device.Kind}: hidden flip swapchain creation, readiness, resize and generation checks passed; last status {outcome}.");
        }
        finally { if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error()); }
    }

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWindowExW(uint extendedStyle, string className, string windowName,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(nint window);
}
