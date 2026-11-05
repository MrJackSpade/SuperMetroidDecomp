using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The two pixel rules every installed sprite sheet's frames are held to (#1513), read from each sheet's installed ink
/// plane, the palette-index image beside its picture that records which colour of which colour set each pixel inks.
/// <list type="bullet">
/// <item><b>Empty frame.</b> A frame is empty when it has no rectangle, or when no pixel of its rectangle inks a colour
/// other than colour zero, as the image's own colour-zero rule (<see cref="SpriteImage.IsColourZero"/>) reads it: the
/// frame has no opaque pixel and draws nothing, so the draw path that names it should draw nothing instead.</item>
/// <item><b>Frame inking an empty colour row.</b> A frame inks an empty colour row when one of its pixels inks a colour
/// other than colour zero of a colour set (a palette row: sixteen colours, or four for a HUD or room-effect image)
/// whose colours in the sheet's standard colours are all black beyond colour zero: the row the sheet's colouring leaves
/// undefined, so the pixel shows black rather than the colour its artwork names.</item>
/// </list>
/// Each sheet's ink plane is read from its installed file once and dropped after its frames are checked, in parallel
/// across sheets: the fixtures' images keep their decoded pixels once read, and every enemy sheet's held at once would
/// cost gigabytes.
/// </summary>
internal static class SheetInkRules
{
    /// <summary>Colours in one of the four-colour sets a HUD or room effect image is inked in.</summary>
    private const int TwoBppSetColours = 4;

    /// <summary>The distinct inks a pixel's palette-index byte can hold.</summary>
    private const int InkValues = byte.MaxValue + 1;

    /// <summary>
    /// Fails, listing every offending frame, when a frame of <paramref name="sheets"/> is empty or inks an empty colour
    /// row; reports how many frames passed otherwise.
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
        string[] emptyRows = [.. findings.SelectMany(image => image.EmptyRowFrames)];
        AssertTrue(empty.Length == 0 && emptyRows.Length == 0,
            $"{empty.Length} {family} sheet frames have no opaque pixel and {emptyRows.Length} ink an empty colour row:"
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, empty.Concat(emptyRows))}");
        Console.WriteLine($"{family} sheet inks: {findings.Sum(image => image.Frames)} frames of {images.Length} installed images each draw a pixel and ink only filled colour rows.");
    }

    /// <summary>Checks every frame cut from one image, which <paramref name="users"/> all show.</summary>
    private static ImageFindings CheckImage(string directory, LabelledSheet[] users)
    {
        SpriteImage image = users[0].Sheet.Image;
        byte[] inks = InkPlane(directory, image);
        int setColours = image.Colours == SpriteImageColours.TwoBppBackground ? TwoBppSetColours : ImageColourGroup.Colours;
        bool[] emptySets = EmptySets(image.StandardColours, setColours);
        var colourZero = new bool[InkValues];
        for (int ink = 0; ink < InkValues; ink++)
            colourZero[ink] = image.IsColourZero(new SpriteImageInk(ink, 0));
        string owner = users.Length == 1 ? users[0].Label : $"{users[0].Label} and {users.Length - 1} more";
        var emptyFrames = new List<string>();
        var emptyRowFrames = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (SpriteSheet sheet in users.Select(user => user.Sheet).Distinct())
        {
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (!seen.Add(name))
                    continue;
                (bool draws, int[] inkedEmptySets) = Inspect(inks, image, frame, setColours, colourZero, emptySets);
                if (!draws)
                    emptyFrames.Add($"{owner} ({image.Name}) frame {name} has no opaque pixel");
                if (inkedEmptySets.Length > 0)
                    emptyRowFrames.Add($"{owner} ({image.Name}) frame {name} inks empty colour rows {string.Join(", ", inkedEmptySets)}");
            }
        }
        return new ImageFindings(seen.Count, emptyFrames, emptyRowFrames);
    }

    /// <summary>
    /// Whether <paramref name="frame"/> inks any pixel other than colour zero, and the empty colour sets its pixels ink,
    /// in rising order.
    /// </summary>
    /// <exception cref="InvalidDataException">A pixel inks a colour set the sheet's standard colours do not hold.</exception>
    private static (bool Draws, int[] InkedEmptySets) Inspect(byte[] inks, SpriteImage image, SpriteSheetFrame frame, int setColours,
        bool[] colourZero, bool[] emptySets)
    {
        bool draws = false;
        var inkedEmptySets = new SortedSet<int>();
        int width = image.Size.X;
        for (int row = 0; row < frame.Size.Y; row++)
        {
            int line = (frame.Corner.Y + row) * width + frame.Corner.X;
            for (int column = 0; column < frame.Size.X; column++)
            {
                int ink = inks[line + column];
                if (colourZero[ink])
                    continue;
                draws = true;
                int set = ink / setColours;
                if (set >= emptySets.Length)
                    throw new InvalidDataException($"{image} inks colour {ink}, past its {image.StandardColours.Length} standard colours.");
                if (emptySets[set])
                    inkedEmptySets.Add(set);
            }
        }
        return (draws, [.. inkedEmptySets]);
    }

    /// <summary>Whether each colour set of <paramref name="colours"/> holds only black beyond its colour zero.</summary>
    private static bool[] EmptySets(ReadOnlySpan<RgbColor> colours, int setColours)
    {
        var empty = new bool[colours.Length / setColours];
        for (int set = 0; set < empty.Length; set++)
        {
            bool black = true;
            for (int colour = 1; colour < setColours; colour++)
                black &= colours[set * setColours + colour] == RgbColor.Black;
            empty[set] = black;
        }
        return empty;
    }

    /// <summary>
    /// The ink of each pixel of <paramref name="image"/>, row by row from the top, read from its installed palette-index
    /// image, whose red channel holds each pixel's ink.
    /// </summary>
    /// <exception cref="InvalidDataException">The palette-index image is malformed or not the size of the sheet image.</exception>
    private static byte[] InkPlane(string directory, SpriteImage image)
    {
        string path = Path.Combine(directory, SpriteSheetStore.PaletteFile(image.Name));
        RgbaPixel[] plane;
        Extent16<ImagePixels> size;
        using (FileStream file = File.OpenRead(path))
            plane = RgbaPng.Read(file, out size);
        if (size != image.Size)
            throw new InvalidDataException($"Sheet ink plane {path} is {size.X}x{size.Y}, not the size of {image}.");
        var inks = new byte[plane.Length];
        for (int pixel = 0; pixel < plane.Length; pixel++)
            inks[pixel] = plane[pixel].R;
        return inks;
    }

    /// <summary>What one image's frames showed.</summary>
    /// <param name="Frames">How many distinct frames it was checked for.</param>
    /// <param name="EmptyFrames">Each frame with no opaque pixel.</param>
    /// <param name="EmptyRowFrames">Each frame inking an empty colour row, with the rows.</param>
    private sealed record ImageFindings(int Frames, IReadOnlyList<string> EmptyFrames, IReadOnlyList<string> EmptyRowFrames);
}
