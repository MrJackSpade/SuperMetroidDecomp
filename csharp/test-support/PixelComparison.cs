using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

/// <summary>Exact diagnostic comparison shared by GPU fixtures; failures preserve all reproduction inputs.</summary>
internal static class PixelComparison
{
    internal static void Verify(RenderFrameSnapshot packet, Rgba32[] expected, Rgba32[] actual, string context)
    {
        if (expected.Length != packet.Size.X * packet.Size.Y || actual.Length != expected.Length)
            throw new InvalidDataException($"{context}: unexpected frame dimensions.");
        int count = 0, first = -1, left = packet.Size.X, top = packet.Size.Y, right = -1, bottom = -1;
        var difference = new Rgba32[expected.Length];
        for (int i = 0; i < expected.Length; i++)
        {
            bool mismatch = expected[i] != actual[i];
            difference[i] = mismatch ? new(255, 0, 255) : new(0, 0, 0);
            if (!mismatch) continue;
            count++; if (first < 0) first = i;
            int x = i % packet.Size.X, y = i / packet.Size.X;
            left = Math.Min(left, x); right = Math.Max(right, x);
            top = Math.Min(top, y); bottom = Math.Max(bottom, y);
        }
        if (count == 0) return;
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "render-comparison", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, "frame.smframe"), RenderFrameSnapshotCodec.Serialize(packet));
        PngWriterTooling.WriteRgba(Path.Combine(directory, "expected.png"), packet.Size.X, packet.Size.Y, expected);
        PngWriterTooling.WriteRgba(Path.Combine(directory, "actual.png"), packet.Size.X, packet.Size.Y, actual);
        PngWriterTooling.WriteRgba(Path.Combine(directory, "difference.png"), packet.Size.X, packet.Size.Y, difference);
        File.WriteAllText(Path.Combine(directory, "comparison.json"), JsonSerializer.Serialize(new {
            Context = context, packet.Identity, MismatchedPixels = count,
            FirstX = first % packet.Size.X, FirstY = first / packet.Size.X,
            Expected = expected[first].ToString(), Actual = actual[first].ToString(),
            Bounds = new { Left = left, Top = top, RightInclusive = right, BottomInclusive = bottom }
        }, new JsonSerializerOptions { WriteIndented = true }));
        throw new InvalidOperationException($"{context}: {count} pixels differ; first ({first % packet.Size.X},{first / packet.Size.X}); artifacts: {directory}");
    }
}
