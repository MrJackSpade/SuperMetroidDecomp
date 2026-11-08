using System.Security.Cryptography;
using System.Text;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds the installed spawned-object sheets to the rule that common art is held once (#1513, owner flags 20 and 28): no
/// two installed sheets hold the same frame in the same colouring with identical pixels. A common sprite, such as an
/// explosion, a drop or a projectile, is pictured once in each colouring it is drawn in, and every graphics set that draws
/// it reads that one picture.
/// <para>
/// A frame's picture is compared by everything a draw of it shows: its name, rectangle size, origin and depth, each
/// pixel's colour and ink marks, and its colouring (#1544). Its colouring is the colours of the sheet's own the picture's
/// pixels show, in the order the picture first shows them, each pixel naming its colour by that order, and what each
/// shade shows of them: a sheet's colours and shades also cover its other pictures, and those say nothing about this one.
/// A shade counts when it recolours one of those colours or blends the whole draw.
/// </para>
/// Each sheet's image and palette map are read from its installed files and dropped after its frames are fingerprinted,
/// in parallel across sheets.
/// </summary>
internal static class SpawnedArtHeldOnceVerification
{
    /// <summary>Fails, naming every frame two or more installed spawned-object sheets hold alike, with the sheets holding it.</summary>
    internal static void VerifySpawnedArtHeldOnce()
    {
        string directory = RepositoryInstallation.Installation.EnemySheetDirectory;
        SpriteSheet[] sheets = [.. InstalledFixtures.EnemySheets.spawned.Values.SelectMany(SheetsOf)
            .Distinct<SpriteSheet>(ReferenceEqualityComparer.Instance).OrderBy(sheet => sheet.Image.Name, StringComparer.Ordinal)];
        AssertTrue(sheets.Length > 0, "the installation holds spawned-object sheets");
        var pictures = new HeldPicture[sheets.Length][];
        Parallel.For(0, sheets.Length, index => pictures[index] = Pictures(directory, sheets[index]));
        var holders = new Dictionary<string, List<HeldPicture>>(StringComparer.Ordinal);
        foreach (HeldPicture picture in pictures.SelectMany(held => held))
        {
            if (!holders.TryGetValue(picture.Fingerprint, out List<HeldPicture>? held))
                holders.Add(picture.Fingerprint, held = []);
            held.Add(picture);
        }
        string[] repeated = [.. holders.Values.Where(held => held.Count > 1)
            .Select(held => $"frame {held[0].Frame} is held alike by {held.Count} sheets: {string.Join(", ", held.Select(picture => picture.Sheet))}")
            .Order(StringComparer.Ordinal)];
        int copies = holders.Values.Where(held => held.Count > 1).Sum(held => held.Count - 1);
        AssertTrue(repeated.Length == 0,
            $"{repeated.Length} spawned-object frames are held more than once in one colouring with identical pixels, {copies} extra copies across "
            + $"{sheets.Length} installed sheets:{Environment.NewLine}{string.Join(Environment.NewLine, repeated)}");
        Console.WriteLine($"Spawned art held once: {holders.Count} distinct frame pictures of {sheets.Length} installed spawned-object sheets are each held by one sheet.");
    }

    /// <summary>The installed sheets a set's spawned-object pictures for one owner are on: its own sheet and the common sheets.</summary>
    private static SpriteSheet[] SheetsOf(SpawnedObjectSheet spawned) => [.. spawned.Sheets];

    /// <summary>Each pictured frame of <paramref name="sheet"/> with the fingerprint of everything a draw of it shows.</summary>
    /// <exception cref="InvalidDataException">An installed plane is malformed or not the sheet's size.</exception>
    private static HeldPicture[] Pictures(string directory, SpriteSheet sheet)
    {
        SpriteImage image = sheet.Image;
        RgbaPixel[] pixels = Plane(directory, SpriteSheetStore.ImageFile(image.Name), image);
        SpriteImageInk[] inks = Array.ConvertAll(Plane(directory, SpriteSheetStore.PaletteFile(image.Name), image), InstalledSheetColouring.InkOf);
        KeyValuePair<PictureShade, ShadeInks>[] shades = [.. image.Shades.OrderBy(shade => shade.Key.Key, StringComparer.Ordinal)];
        return [.. sheet.frames.OrderBy(frame => frame.Key, StringComparer.Ordinal).Select(frame =>
            new HeldPicture(frame.Key, image.Name, Fingerprint(frame.Key, frame.Value, image, pixels, inks, shades)))];
    }

    /// <summary>
    /// The fingerprint of frame <paramref name="name"/> as a draw shows it: its rectangle's size, origin and depth, each
    /// pixel's colour and ink marks, the colours of the sheet's own its pixels show, each pixel naming its colour by the
    /// order the frame first shows them, and what each shade shows of those colours.
    /// </summary>
    private static string Fingerprint(string name, SpriteSheetFrame frame, SpriteImage image, RgbaPixel[] pixels, SpriteImageInk[] inks,
        KeyValuePair<PictureShade, ShadeInks>[] shades)
    {
        // The sheet's colours the frame shows, by their order in the frame.
        var shownColours = new List<int>();
        var orderOf = new Dictionary<int, int>();
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(name);
            writer.Write(frame.Size.X);
            writer.Write(frame.Size.Y);
            writer.Write(frame.Origin.X);
            writer.Write(frame.Origin.Y);
            writer.Write(frame.Depth?.ToString() ?? "none");
            int width = image.Size.X;
            for (int row = 0; row < frame.Size.Y; row++)
            {
                for (int column = 0; column < frame.Size.X; column++)
                {
                    int at = (frame.Corner.Y + row) * width + frame.Corner.X + column;
                    RgbaPixel pixel = pixels[at];
                    SpriteImageInk ink = inks[at];
                    writer.Write(pixel.R);
                    writer.Write(pixel.G);
                    writer.Write(pixel.B);
                    writer.Write(pixel.A);
                    writer.Write(ink.Clear);
                    writer.Write(ink.Blends);
                    writer.Write(ink.FrontTile);
                    if (ink.Colour is not int colour)
                    {
                        writer.Write(-1);
                        continue;
                    }
                    if (!orderOf.TryGetValue(colour, out int order))
                    {
                        orderOf.Add(colour, order = shownColours.Count);
                        shownColours.Add(colour);
                    }
                    writer.Write(order);
                }
            }
            ReadOnlySpan<RgbColor> colours = image.StandardColours;
            foreach (int colour in shownColours)
                WriteColour(writer, colours[colour]);
            foreach ((PictureShade shade, ShadeInks shown) in shades)
            {
                int[] entries = [.. Enumerable.Range(0, shown.Inks.Length).Where(entry => orderOf.ContainsKey(shown.Inks[entry]))
                    .OrderBy(entry => orderOf[shown.Inks[entry]])];
                if (entries.Length == 0 && !shown.Blends)
                    continue;
                writer.Write(shade.Key);
                writer.Write(shown.Blends);
                writer.Write(entries.Length);
                foreach (int entry in entries)
                {
                    writer.Write(orderOf[shown.Inks[entry]]);
                    WriteColour(writer, shown.Colours[entry]);
                }
            }
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    /// <summary>Writes a colour's three channels.</summary>
    private static void WriteColour(BinaryWriter writer, RgbColor colour)
    {
        writer.Write(colour.Red);
        writer.Write(colour.Green);
        writer.Write(colour.Blue);
    }

    /// <summary>One of <paramref name="image"/>'s installed planes, row by row from the top.</summary>
    /// <exception cref="InvalidDataException">The file is malformed or not the size of the image.</exception>
    private static RgbaPixel[] Plane(string directory, string file, SpriteImage image)
    {
        string path = Path.Combine(directory, file);
        RgbaPixel[] plane;
        Extent16<ImagePixels> size;
        using (FileStream stream = File.OpenRead(path))
            plane = RgbaPng.Read(stream, out size);
        if (size != image.Size)
            throw new InvalidDataException($"Sheet plane {path} is {size.X}x{size.Y}, not the size of {image}.");
        return plane;
    }

    /// <summary>One frame an installed sheet pictures.</summary>
    /// <param name="Frame">The frame's name.</param>
    /// <param name="Sheet">The installed sheet holding it.</param>
    /// <param name="Fingerprint">The fingerprint of everything a draw of it shows.</param>
    private sealed record HeldPicture(string Frame, string Sheet, string Fingerprint);

    /// <summary>Runs the installed spawned-object sheets' held-once rule.</summary>
    [VerificationCommand("--spawned-art-held-once")]
    internal static int SpawnedArtHeldOnceCommand()
    {
        Suite(nameof(VerifySpawnedArtHeldOnce), VerifySpawnedArtHeldOnce);
        return 0;
    }
}
