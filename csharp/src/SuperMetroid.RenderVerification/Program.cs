using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

NativeConsoleErrors.DisableDialogs();
try
{
    if (args.Length == 4 && args[0] == "--compare" && args[2] == "--device")
    {
        D3D11DeviceKind kind = args[3] switch {
            "hardware" => D3D11DeviceKind.Hardware, "warp" => D3D11DeviceKind.Warp,
            _ => throw new ArgumentException("Device must be hardware or warp.") };
        var frame = RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(args[1]));
        using var device = new D3D11RenderDevice(kind);
        using var renderer = new D3D11FrameRenderer(device);
        PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
            $"{kind}: {device.AdapterDescription}; {Path.GetFullPath(args[1])}");
        Console.WriteLine($"Exact match: {kind}, {device.AdapterDescription}, {frame.Width}x{frame.Height}.");
        return;
    }
    if (args.Length != 1 || args[0] is not ("--solid-smoke" or "--tile-smoke" or "--obj-smoke" or "--mode7-smoke" or "--window-smoke" or "--ordinary-smoke" or "--scene-window-smoke" or "--retail-frontend" or "--retail-intro" or "--retail-transitions" or "--display-smoke" or "--swapchain-smoke"))
        throw new ArgumentException("Usage: SuperMetroid.RenderVerification --solid-smoke | --tile-smoke | --obj-smoke | --mode7-smoke | --window-smoke | --ordinary-smoke | --scene-window-smoke | --retail-frontend | --retail-intro | --retail-transitions | --display-smoke | --swapchain-smoke | --compare <frame.smframe> --device hardware|warp");
    foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
    {
        using var device = new D3D11RenderDevice(kind);
        using var renderer = new D3D11FrameRenderer(device);
        if (args[0] == "--swapchain-smoke") { SwapchainTests.Run(device, renderer); SwapchainTests.RunWorker(device); continue; }
        if (args[0] == "--display-smoke") { DisplayPassTests.Run(device, renderer); continue; }
        if (args[0] == "--retail-transitions") { RetailTransitionTests.Run(device, renderer); continue; }
        if (args[0] == "--retail-intro") { RetailCinematicTests.Run(device, renderer); continue; }
        if (args[0] == "--retail-frontend") { RetailFrontendTests.Run(device, renderer); continue; }
        if (args[0] == "--scene-window-smoke") { WindowSceneSmokeTests.Run(device, renderer); continue; }
        if (args[0] == "--ordinary-smoke") { OrdinarySmokeTests.Run(device, renderer); continue; }
        if (args[0] == "--window-smoke") { ColorWindowSmokeTests.Run(device, renderer); continue; }
        if (args[0] == "--mode7-smoke") { Mode7SmokeTests.Run(device, renderer); continue; }
        if (args[0] == "--obj-smoke") { ObjectSmokeTests.Run(device, renderer); continue; }
        if (args[0] == "--tile-smoke") { TileSmokeTests.Run(device, renderer); continue; }
        bool rejected = false;
        try { renderer.Readback(); } catch (InvalidOperationException) { rejected = true; }
        if (!rejected) throw new InvalidOperationException("Readback before submission was accepted.");
        RenderFrameSnapshot? last = null;
        for (int i = 0; i < 64; i++)
        {
            last = new(new(i + 1, 1, 0), new Rgba32((byte)i, 73, 129));
            renderer.Render(last);
        }
        PixelComparison.Verify(last!, SoftwareFrameSnapshotRenderer.Render(last!), renderer.Readback(),
            $"{kind}: 64 submissions with deferred readback");
        PixelComparison.Verify(last!, SoftwareFrameSnapshotRenderer.Render(last!), renderer.Readback(),
            $"{kind}: repeat deferred readback");
        Console.WriteLine($"{kind}: deferred submission/readback and uninitialized readback guard passed.");
        int samples = 0;
        foreach (byte alpha in new byte[] { 0, 1, 128, 255 })
        for (byte brightness = 0; brightness <= 15; brightness++)
        {
            var frame = new RenderFrameSnapshot(new(++samples, 1, 0), new Rgba32(231, 123, 47, alpha), new byte[] { brightness, 11 });
            Rgba32[] expected = SoftwareFrameSnapshotRenderer.Render(frame);
            Rgba32[] actual = renderer.RenderForReadback(frame);
            PixelComparison.Verify(frame, expected, actual, $"{kind}: solid compute sample {samples}");
        }
        // A wrong-thread call must fail before issuing any D3D context operation.
        Exception? wrongThread = Task.Run(() =>
        {
            try { renderer.RenderForReadback(new(new(1, 1, 0), new Rgba32(0, 0, 0))); return null; }
            catch (Exception exception) { return exception; }
        }).GetAwaiter().GetResult();
        if (wrongThread is not InvalidOperationException) throw new InvalidOperationException("Missing render-owner thread guard.");
        Console.WriteLine($"{kind}: {device.AdapterDescription}; {samples} exact compute/readback frames passed; thread guard passed.");
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    Environment.ExitCode = 1;
}
