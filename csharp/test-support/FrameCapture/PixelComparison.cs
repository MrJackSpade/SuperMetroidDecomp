using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.FrameCapture;

/// <summary>Exact diagnostic comparison shared by GPU fixtures; failures preserve all reproduction inputs.</summary>
internal static class PixelComparison
{
    /// <summary>Checks exact RGBA pixels and reports mismatch count, first sample, and inclusive bounds with fixture identity.</summary>
    internal static void Verify(DisplayPacket packet, RgbaPixel[] expected, RgbaPixel[] actual, string context)
    {
        if (expected.Length != packet.Size.X * packet.Size.Y || actual.Length != expected.Length)
            throw new InvalidDataException($"{context}: unexpected frame dimensions.");
        int count = 0, first = -1, left = packet.Size.X, top = packet.Size.Y, right = -1, bottom = -1;
        var difference = new RgbaPixel[expected.Length];
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
        string images = Save(packet, context, expected, actual, difference);
        throw new InvalidOperationException($"{context} ({packet.Identity}): {count} pixels differ; first ({first % packet.Size.X},{first / packet.Size.X}) " +
            $"expected {expected[first]} actual {actual[first]}; bounds ({left},{top})-({right},{bottom}) inclusive; images in {images}.");
    }

    /// <summary>
    /// Writes the expected, actual and difference frames at their native size beside the test's binaries, in a folder
    /// named for the failure, and returns its full path.
    /// </summary>
    private static string Save(DisplayPacket packet, string context, RgbaPixel[] expected, RgbaPixel[] actual, RgbaPixel[] difference)
    {
        string name = string.Concat($"{context} {packet.Identity.Sequence}".Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        string directory = Path.Combine(AppContext.BaseDirectory, "pixel-failures", name);
        Directory.CreateDirectory(directory);
        var size = new Extent16<ImagePixels>(packet.Size.X, packet.Size.Y);
        foreach ((string file, RgbaPixel[] pixels) in new[] { ("expected.png", expected), ("actual.png", actual), ("difference.png", difference) })
        {
            using FileStream output = File.Create(Path.Combine(directory, file));
            SuperMetroid.AssetExtraction.Artwork.RgbaPng.Write(output, size, pixels);
        }
        return directory;
    }
}
