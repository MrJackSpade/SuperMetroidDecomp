using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

NativeConsoleErrors.DisableDialogs();
try
{
    if (args.Length != 1 || args[0] != "--solid-smoke")
        throw new ArgumentException("Usage: SuperMetroid.RenderVerification --solid-smoke");
    foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
    {
        using var device = new D3D11RenderDevice(kind);
        using var renderer = new D3D11SolidRenderer(device);
        int samples = 0;
        foreach (byte alpha in new byte[] { 0, 1, 128, 255 })
        for (byte brightness = 0; brightness <= 15; brightness++)
        {
            var frame = new RenderFrameSnapshot(new(++samples, 1, 0), new Rgba32(231, 123, 47, alpha), new byte[] { brightness, 11 });
            Rgba32[] expected = SoftwareFrameSnapshotRenderer.Render(frame);
            Rgba32[] actual = renderer.RenderForReadback(frame);
            if (!expected.AsSpan().SequenceEqual(actual)) throw new InvalidOperationException($"{kind}: solid compute mismatch at sample {samples}.");
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
