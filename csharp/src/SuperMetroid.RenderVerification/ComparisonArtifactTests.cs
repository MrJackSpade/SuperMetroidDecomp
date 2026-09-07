using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Deliberate mismatch verifies the diagnostic artifact pipeline, not just its success path.</summary>
internal static class ComparisonArtifactTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(31, 73, 127));
        var expected = SoftwareFrameSnapshotRenderer.Render(packet);
        var actual = expected.ToArray();
        int first = packet.Width + 2, last = 3 * packet.Width + 5;
        actual[first] = new(255, 0, 0);
        actual[last] = new(0, 255, 0);
        PixelComparisonException failure;
        try
        {
            PixelComparison.Verify(packet, expected, actual, "intentional artifact verification");
            throw new InvalidOperationException("Deliberate pixel mismatch was accepted.");
        }
        catch (PixelComparisonException error) { failure = error; }
        string directory = failure.ArtifactDirectory;
        // Only remove this freshly-created diagnostic directory after successful
        // verification. A failing artifact test deliberately preserves its evidence.
        using (var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "comparison.json"))))
        {
            var root = json.RootElement;
            var bounds = root.GetProperty("Bounds");
            if (root.GetProperty("MismatchedPixels").GetInt32() != 2 ||
                root.GetProperty("FirstX").GetInt32() != 2 || root.GetProperty("FirstY").GetInt32() != 1 ||
                bounds.GetProperty("Left").GetInt32() != 2 || bounds.GetProperty("Top").GetInt32() != 1 ||
                bounds.GetProperty("RightInclusive").GetInt32() != 5 || bounds.GetProperty("BottomInclusive").GetInt32() != 3 ||
                root.GetProperty("Expected").GetString() != expected[first].ToString() ||
                root.GetProperty("Actual").GetString() != actual[first].ToString())
                throw new InvalidOperationException("Mismatch metadata lost exact coordinates, bounds or colors.");
        }
        var difference = Enumerable.Repeat(new Rgba32(0, 0, 0), expected.Length).ToArray();
        difference[first] = difference[last] = new(255, 0, 255);
        foreach (var (name, pixels) in new[] { ("expected.png", expected), ("actual.png", actual), ("difference.png", difference) })
        {
            string check = Path.Combine(directory, "check.png");
            PngWriter.WriteRgba(check, packet.Width, packet.Height, pixels);
            if (!File.ReadAllBytes(check).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(directory, name))))
                throw new InvalidOperationException($"Mismatch artifact {name} contains the wrong image.");
        }
        byte[] serialized = File.ReadAllBytes(Path.Combine(directory, "frame.smframe"));
        if (!serialized.AsSpan().SequenceEqual(RenderFrameSnapshotCodec.Serialize(packet)))
            throw new InvalidOperationException("Diagnostic packet differs from the failed comparison input.");
        var replay = RenderFrameSnapshotCodec.Deserialize(serialized);
        PixelComparison.Verify(replay, expected, renderer.RenderForReadback(replay), "artifact packet replay");
        Directory.Delete(directory, recursive: true);
        Console.WriteLine($"{device.Kind}: intentional mismatch metadata/images/packet replay verified; test artifacts removed.");
    }
}
