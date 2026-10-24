using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Harness;
using SuperMetroid.TestSupport.FrameCapture;

/// <summary>
/// A deliberate mismatch verifies the comparison's failure evidence, not just its success path. Its exception message
/// carries the mismatch count, first pixel and colors, the inclusive bounds, and the folder it wrote the expected, actual
/// and difference frames to, so the failure can be opened and seen.
/// </summary>
internal static class ComparisonArtifactTests
{
    /// <summary>Confirms that two deliberately altered pixels produce exact
    /// mismatch evidence, then verifies the unaltered frame through GPU readback.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var packet = new DisplayPacket(new(1, 1, 0), new Rgba32(31, 73, 127));
        var expected = DisplayRendering.Render(packet);
        var actual = expected.ToArray();
        int first = packet.Size.X + 2, last = 3 * packet.Size.X + 5;
        actual[first] = new(255, 0, 0);
        actual[last] = new(0, 255, 0);
        string? failure = null;
        try { PixelComparison.Verify(packet, expected, actual, "intentional mismatch"); }
        catch (InvalidOperationException error) { failure = error.Message; }
        if (failure is null)
            throw new InvalidOperationException("Deliberate pixel mismatch was accepted.");
        string evidence = $"2 pixels differ; first (2,1) expected {expected[first]} actual {actual[first]}; bounds (2,1)-(5,3) inclusive; images in ";
        int at = failure.IndexOf(evidence, StringComparison.Ordinal);
        if (at < 0 || !failure.EndsWith('.'))
            throw new InvalidOperationException($"Mismatch evidence lost exact count, coordinates, colors, bounds or images: {failure}");
        string images = failure[(at + evidence.Length)..^1];
        foreach (string file in new[] { "expected.png", "actual.png", "difference.png" })
        {
            if (!File.Exists(Path.Combine(images, file)))
                throw new InvalidOperationException($"Mismatch evidence names {images}, which holds no {file}.");
        }
        PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "mismatch fixture renders on the GPU");
        Console.WriteLine($"{device.Kind}: intentional mismatch reports exact count, first pixel, colors and bounds, and writes its frames to {images}.");
    }
}
