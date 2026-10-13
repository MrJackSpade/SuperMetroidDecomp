using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>
/// A deliberate mismatch verifies the comparison's failure evidence, not just its success path. The
/// comparison writes no files; its exception message carries the mismatch count, first pixel and colors,
/// and the inclusive bounds.
/// </summary>
internal static class ComparisonArtifactTests
{
    /// <summary>Confirms that two deliberately altered pixels produce exact
    /// mismatch evidence, then verifies the unaltered frame through GPU readback.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(31, 73, 127));
        var expected = SoftwareFrameSnapshotRenderer.Render(packet);
        var actual = expected.ToArray();
        int first = packet.Size.X + 2, last = 3 * packet.Size.X + 5;
        actual[first] = new(255, 0, 0);
        actual[last] = new(0, 255, 0);
        string? failure = null;
        try { PixelComparison.Verify(packet, expected, actual, "intentional mismatch"); }
        catch (InvalidOperationException error) { failure = error.Message; }
        if (failure is null)
            throw new InvalidOperationException("Deliberate pixel mismatch was accepted.");
        string evidence = $"2 pixels differ; first (2,1) expected {expected[first]} actual {actual[first]}; bounds (2,1)-(5,3) inclusive.";
        if (!failure.EndsWith(evidence, StringComparison.Ordinal))
            throw new InvalidOperationException($"Mismatch evidence lost exact count, coordinates, colors or bounds: {failure}");
        PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "mismatch fixture renders on the GPU");
        Console.WriteLine($"{device.Kind}: intentional mismatch reports exact count, first pixel, colors and bounds.");
    }
}
