using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The pixel rule every installed sprite sheet's frames are held to (#1513), read from each sheet's installed picture:
/// <list type="bullet">
/// <item><b>Empty frame.</b> A frame is empty when it has no rectangle, or when every pixel of its rectangle is clear, its
/// alpha zero (#1633): the frame has no opaque pixel and draws nothing, so the draw path that names it should draw nothing
/// instead.</item>
/// </list>
/// Each sheet's picture is read from its installed file once and dropped after its frames are checked, in parallel across
/// sheets: the fixtures' images keep their decoded pixels once read, and every enemy sheet's held at once would cost
/// gigabytes.
/// </summary>
internal static class SheetPixelRules
{
    /// <summary>
    /// Fails, listing every offending frame, when a frame of <paramref name="sheets"/> is empty; reports how many frames
    /// passed otherwise.
    /// </summary>
    /// <param name="family">The family the sheets belong to, which names them in the report.</param>
    /// <param name="directory">The installed directory holding the sheets' files.</param>
    /// <param name="sheets">The family's installed sheets, each with the name it is reported by.</param>
    internal static void Assert(string family, string directory, IReadOnlyList<LabelledSheet> sheets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(family);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(sheets);
        AssertTrue(sheets.Count > 0, $"the installation holds {family} sheets");
        LabelledSheet[][] images = [.. sheets.GroupBy(sheet => sheet.Sheet.Image).Select(users => users.ToArray())];
        var findings = new ImageFindings[images.Length];
        Parallel.For(0, images.Length, index => findings[index] = CheckImage(directory, images[index]));
        string[] empty = [.. findings.SelectMany(image => image.EmptyFrames)];
        AssertTrue(empty.Length == 0,
            $"{empty.Length} {family} sheet frames have no opaque pixel:{Environment.NewLine}{string.Join(Environment.NewLine, empty)}");
        Console.WriteLine($"{family} sheet pixels: {findings.Sum(image => image.Frames)} frames of {images.Length} installed images each draw a pixel.");
    }

    /// <summary>Checks every frame cut from one image, which <paramref name="users"/> all show.</summary>
    private static ImageFindings CheckImage(string directory, LabelledSheet[] users)
    {
        SpriteImage image = users[0].Sheet.Image;
        RgbaPixel[] pixels = PixelPlane(directory, image);
        string owner = users.Length == 1 ? users[0].Label : $"{users[0].Label} and {users.Length - 1} more";
        var emptyFrames = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (SpriteSheet sheet in users.Select(user => user.Sheet).Distinct())
        {
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (!seen.Add(name))
                    continue;
                if (!Draws(pixels, image, frame))
                    emptyFrames.Add($"{owner} ({image.Name}) frame {name} has no opaque pixel");
            }
        }
        return new ImageFindings(seen.Count, emptyFrames);
    }

    /// <summary>Whether <paramref name="frame"/> has any pixel that is not clear.</summary>
    private static bool Draws(RgbaPixel[] pixels, SpriteImage image, SpriteSheetFrame frame)
    {
        int width = image.Size.X;
        for (int row = 0; row < frame.Size.Y; row++)
        {
            int line = (frame.Corner.Y + row) * width + frame.Corner.X;
            for (int column = 0; column < frame.Size.X; column++)
            {
                if (pixels[line + column].A != 0)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// The pixels of <paramref name="image"/>, row by row from the top, read from its installed picture file as the loader
    /// reads it, without keeping them in the fixture's image.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture is malformed or not the size of the sheet image.</exception>
    internal static RgbaPixel[] PixelPlane(string directory, SpriteImage image)
    {
        string path = Path.Combine(directory, SpriteSheetStore.ImageFile(image.Name));
        RgbaPixel[] plane;
        Extent16<ImagePixels> size;
        using (FileStream file = File.OpenRead(path))
            plane = RgbaPng.Read(file, out size);
        if (size != image.Size)
            throw new InvalidDataException($"Sheet picture {path} is {size.X}x{size.Y}, not the size of {image}.");
        return plane;
    }

    /// <summary>
    /// The size and pixels of <paramref name="corner"/>'s rectangle of <paramref name="image"/>, spelled for telling
    /// pictures apart: each pixel's colour and alpha, its recolour region and whether it blends (#1633), so two rectangles
    /// spelled alike show alike under every shade and every draw.
    /// </summary>
    internal static string Spelled(SpriteImage image, Position16<ImagePixels> corner, Extent16<ImagePixels> size)
    {
        ArgumentNullException.ThrowIfNull(image);
        ReadOnlySpan<RgbaPixel> pixels = image.Pixels;
        ReadOnlySpan<int> regions = image.RecolourRegions;
        ReadOnlySpan<bool> blending = image.Blending;
        int width = image.Size.X;
        var bytes = new byte[size.X * size.Y * 9];
        int at = 0;
        for (int row = 0; row < size.Y; row++)
        {
            for (int column = 0; column < size.X; column++)
            {
                int pixel = (corner.Y + row) * width + corner.X + column;
                RgbaPixel shown = pixels[pixel];
                bytes[at++] = shown.R;
                bytes[at++] = shown.G;
                bytes[at++] = shown.B;
                bytes[at++] = shown.A;
                int region = regions.IsEmpty ? 0 : regions[pixel];
                BitConverter.TryWriteBytes(bytes.AsSpan(at, sizeof(int)), region);
                at += sizeof(int);
                bytes[at++] = !blending.IsEmpty && blending[pixel] ? (byte)1 : (byte)0;
            }
        }
        return $"{size.X}x{size.Y}:{Convert.ToBase64String(bytes)}";
    }

    /// <summary>What one image's frames showed.</summary>
    /// <param name="Frames">How many distinct frames it was checked for.</param>
    /// <param name="EmptyFrames">Each frame with no opaque pixel.</param>
    private sealed record ImageFindings(int Frames, IReadOnlyList<string> EmptyFrames);
}
