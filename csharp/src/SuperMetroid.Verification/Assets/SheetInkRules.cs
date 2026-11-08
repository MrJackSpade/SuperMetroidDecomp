using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The pixel rule every installed sprite sheet's frames are held to (#1513), read from each sheet's installed ink plane,
/// the palette map beside its picture that records each pixel's ink (#1544).
/// <list type="bullet">
/// <item><b>Empty frame.</b> A frame is empty when it has no rectangle, or when every pixel of its rectangle is clear, as
/// the image itself marks it (<see cref="SpriteImageInk.Clear"/>): the frame has no opaque pixel and draws nothing, so the
/// draw path that names it should draw nothing instead.</item>
/// </list>
/// Each sheet's ink plane is read from its installed file once and dropped after its frames are checked, in parallel
/// across sheets: the fixtures' images keep their decoded pixels once read, and every enemy sheet's held at once would
/// cost gigabytes.
/// </summary>
internal static class SheetInkRules
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
        Console.WriteLine($"{family} sheet inks: {findings.Sum(image => image.Frames)} frames of {images.Length} installed images each draw a pixel.");
    }

    /// <summary>Checks every frame cut from one image, which <paramref name="users"/> all show.</summary>
    private static ImageFindings CheckImage(string directory, LabelledSheet[] users)
    {
        SpriteImage image = users[0].Sheet.Image;
        SpriteImageInk[] inks = InkPlane(directory, image);
        string owner = users.Length == 1 ? users[0].Label : $"{users[0].Label} and {users.Length - 1} more";
        var emptyFrames = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (SpriteSheet sheet in users.Select(user => user.Sheet).Distinct())
        {
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (!seen.Add(name))
                    continue;
                if (!Draws(inks, image, frame))
                    emptyFrames.Add($"{owner} ({image.Name}) frame {name} has no opaque pixel");
            }
        }
        return new ImageFindings(seen.Count, emptyFrames);
    }

    /// <summary>Whether <paramref name="frame"/> has any pixel that is not clear.</summary>
    private static bool Draws(SpriteImageInk[] inks, SpriteImage image, SpriteSheetFrame frame)
    {
        int width = image.Size.X;
        for (int row = 0; row < frame.Size.Y; row++)
        {
            int line = (frame.Corner.Y + row) * width + frame.Corner.X;
            for (int column = 0; column < frame.Size.X; column++)
            {
                if (!inks[line + column].Clear)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// The ink of each pixel of <paramref name="image"/>, row by row from the top, read from its installed palette map as
    /// the loader reads it (#1544).
    /// </summary>
    /// <exception cref="InvalidDataException">The palette map is malformed or not the size of the sheet image.</exception>
    internal static SpriteImageInk[] InkPlane(string directory, SpriteImage image)
    {
        string path = Path.Combine(directory, SpriteSheetStore.PaletteFile(image.Name));
        RgbaPixel[] plane;
        Extent16<ImagePixels> size;
        using (FileStream file = File.OpenRead(path))
            plane = RgbaPng.Read(file, out size);
        if (size != image.Size)
            throw new InvalidDataException($"Sheet ink plane {path} is {size.X}x{size.Y}, not the size of {image}.");
        return Array.ConvertAll(plane, InstalledSheetColouring.InkOf);
    }

    /// <summary>The installed palette map's bytes of <paramref name="inks"/>, for comparing pictures' inks.</summary>
    internal static byte[] MapBytes(ReadOnlySpan<SpriteImageInk> inks)
    {
        var bytes = new byte[inks.Length * 4];
        for (int pixel = 0; pixel < inks.Length; pixel++)
        {
            RgbaPixel mapped = InstalledSheetColouring.MapPixelOf(inks[pixel]);
            bytes[pixel * 4] = mapped.R;
            bytes[pixel * 4 + 1] = mapped.G;
            bytes[pixel * 4 + 2] = mapped.B;
            bytes[pixel * 4 + 3] = mapped.A;
        }
        return bytes;
    }

    /// <summary>What one image's frames showed.</summary>
    /// <param name="Frames">How many distinct frames it was checked for.</param>
    /// <param name="EmptyFrames">Each frame with no opaque pixel.</param>
    private sealed record ImageFindings(int Frames, IReadOnlyList<string> EmptyFrames);
}
