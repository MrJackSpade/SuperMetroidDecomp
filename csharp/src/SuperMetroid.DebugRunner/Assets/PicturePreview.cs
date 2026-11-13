using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.DebugRunner.Assets;

/// <summary>
/// Developer review of an installation's pictures by tag (#1639). <see cref="Write"/> draws every stored picture exactly
/// once, in the preview of the first sheet showing it (families, then sheets, in the order the store writes them): each
/// picture upscaled by a whole number, boxed in its own margin and labelled with its tag beneath, packed in rows. A stored
/// picture is its pixels only (#1644), so a picture frames show with different maps is still drawn once, and it is the box of
/// what its frames show (#1646), so the preview draws no frame's clear margin. A sheet showing no
/// picture an earlier sheet did not has no preview; the tag lookup names every sheet showing a picture. Transparent pixels
/// show as a checkerboard. <see cref="Describe"/> prints everything a tag resolves to, with the maps each frame shows it
/// with. Neither writes into the installation; a production load never reads a preview.
/// </summary>
internal static class PicturePreview
{
    /// <summary>The whole-number upscale of every picture and of the tag font.</summary>
    private const int Scale = 2;

    /// <summary>The widest a preview's row of pictures grows before the next row starts, in preview pixels.</summary>
    private const int RowWidth = 1024;

    /// <summary>The space between neighbouring cells, in preview pixels.</summary>
    private const int Spacing = 4;

    /// <summary>The preview's background.</summary>
    private static readonly RgbaPixel Background = new(0x30, 0x30, 0x30);

    /// <summary>The two squares of the checkerboard transparent pixels show.</summary>
    private static readonly RgbaPixel[] Checker = [new(0x58, 0x58, 0x58), new(0x70, 0x70, 0x70)];

    /// <summary>The box drawn around every picture.</summary>
    private static readonly RgbaPixel Box = new(0x00, 0xFF, 0x00);

    /// <summary>The tag's text and its strip.</summary>
    private static readonly RgbaPixel Text = new(0xFF, 0xFF, 0xFF);

    /// <summary>The strip behind a tag.</summary>
    private static readonly RgbaPixel Strip = new(0x00, 0x00, 0x00);

    /// <summary>
    /// Writes a preview of every sheet of the installation content in <paramref name="content"/> under
    /// <paramref name="output"/>, one image per sheet showing a picture no earlier sheet did, at <c>&lt;family&gt;/&lt;sheet&gt;.png</c>.
    /// </summary>
    /// <returns>The number of previews written and of pictures drawn, each once.</returns>
    /// <exception cref="IOException"><paramref name="output"/> already exists.</exception>
    /// <exception cref="InvalidDataException">The store or a sheet manifest is malformed, or a frame names a tag the store does not list.</exception>
    internal static (int Sheets, int Pictures) Write(string content, string output)
    {
        if (Directory.Exists(output) || File.Exists(output))
            throw new IOException($"Preview directory {output} already exists; previews are never mixed with older ones.");
        Store store = Store.Open(content);
        int sheets = 0;
        int pictures = 0;
        var drawn = new HashSet<string>(StringComparer.Ordinal);
        using var png = new RgbaPngBuffers();
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(content))
        {
            string[] tags = [.. document.Frames.Values.Where(frame => frame.Picture is not null)
                .Select(frame => frame.Picture!.Tag).Distinct(StringComparer.Ordinal).Where(drawn.Add).Order(StringComparer.Ordinal)];
            if (tags.Length == 0)
                continue;
            (Extent16<ImagePixels> size, RgbaPixel[] pixels) = Draw(store, $"{family}/{sheet}", tags);
            string directory = Path.Combine(output, family);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, sheet + ".png"), RgbaPng.Encode(size, pixels, png));
            sheets++;
            pictures += tags.Length;
        }
        return (sheets, pictures);
    }

    /// <summary>
    /// Prints where the store holds the picture tagged <paramref name="tag"/> and every sheet frame showing it, each group of
    /// frames with where the picture lies in their rectangle (#1646) and the recolour-region and blend maps they show it with
    /// (#1644).
    /// </summary>
    /// <exception cref="InvalidDataException">The store does not list the tag, or the store or a sheet manifest is malformed.</exception>
    internal static void Describe(string content, string tag)
    {
        Store store = Store.Open(content);
        StoredPictureDocument picture = store.Listed(tag, "the lookup");
        Console.WriteLine($"{tag}: {PictureStoreFiles.PageFile(picture.Page)} at ({picture.Corner.X}, {picture.Corner.Y}), " +
            $"{picture.Size.X}x{picture.Size.Y}");
        int shown = 0;
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(content))
        {
            IGrouping<string, string>[] frames = document.Frames.Where(frame => frame.Value.Picture?.Tag == tag)
                .GroupBy(frame => ShownAs(frame.Value), frame => frame.Key, StringComparer.Ordinal)
                .OrderBy(maps => maps.Key, StringComparer.Ordinal)
                .ToArray();
            if (frames.Length == 0)
                continue;
            foreach (IGrouping<string, string> maps in frames)
                Console.WriteLine($"  {family}/{sheet} ({maps.Key}): {string.Join(", ", maps.Order(StringComparer.Ordinal))}");
            shown++;
        }
        Console.WriteLine($"  shown by {shown} sheet(s)");
    }

    /// <summary>
    /// Where <paramref name="frame"/>'s picture lies in its rectangle and the maps the frame shows it with, as the lookup
    /// prints them.
    /// </summary>
    private static string ShownAs(InstalledSheetFrameDocument frame)
    {
        InstalledPictureReferenceDocument reference = frame.Picture
            ?? throw new InvalidOperationException("The lookup describes only frames that show a picture.");
        return $"at ({reference.Offset.X}, {reference.Offset.Y}) of a {frame.Size.X}x{frame.Size.Y} frame, " +
            $"recolour regions {reference.RecolourRegionMap ?? "none"}, blending {reference.BlendMap ?? "none"}";
    }

    /// <summary>The preview of <paramref name="tags"/>: each picture boxed, with its tag beneath, packed into rows.</summary>
    private static (Extent16<ImagePixels> Size, RgbaPixel[] Pixels) Draw(Store store, string sheet, string[] tags)
    {
        int glyph = PictureTagGlyphs.Width * Scale;
        int label = PictureTagGlyphs.Height * Scale + 2;
        var cells = new List<(string Tag, StoredPictureDocument Picture, int X, int Y)>();
        int x = Spacing;
        int y = Spacing;
        int rowHeight = 0;
        int width = 0;
        foreach (string tag in tags)
        {
            StoredPictureDocument picture = store.Listed(tag, sheet);
            int cellWidth = Math.Max(picture.Size.X * Scale + 4, tag.Length * (glyph + Scale) + 2);
            int cellHeight = picture.Size.Y * Scale + 4 + label;
            if (x > Spacing && x + cellWidth > RowWidth)
            {
                x = Spacing;
                y += rowHeight + Spacing;
                rowHeight = 0;
            }
            cells.Add((tag, picture, x, y));
            x += cellWidth + Spacing;
            width = Math.Max(width, x);
            rowHeight = Math.Max(rowHeight, cellHeight);
        }
        int height = y + rowHeight + Spacing;
        var pixels = new RgbaPixel[width * height];
        Array.Fill(pixels, Background);
        foreach ((string tag, StoredPictureDocument picture, int cellX, int cellY) in cells)
        {
            int pictureWidth = picture.Size.X * Scale;
            int pictureHeight = picture.Size.Y * Scale;
            // The box lies one pixel outside the picture, so it never covers a pixel of it.
            for (int column = 0; column < pictureWidth + 4; column++)
            {
                pixels[(cellY * width) + cellX + column] = Box;
                pixels[((cellY + pictureHeight + 3) * width) + cellX + column] = Box;
            }
            for (int row = 0; row < pictureHeight + 4; row++)
            {
                pixels[((cellY + row) * width) + cellX] = Box;
                pixels[((cellY + row) * width) + cellX + pictureWidth + 3] = Box;
            }
            for (int row = 0; row < pictureHeight; row++)
            {
                for (int column = 0; column < pictureWidth; column++)
                {
                    RgbaPixel source = store.PixelAt(picture, column / Scale, row / Scale);
                    RgbaPixel under = Checker[((column / 4) + (row / 4)) % 2];
                    pixels[((cellY + 2 + row) * width) + cellX + 2 + column] = Over(source, under);
                }
            }
            int labelY = cellY + pictureHeight + 4;
            int labelWidth = tag.Length * (glyph + Scale) + 2;
            for (int row = 0; row < label; row++)
                for (int column = 0; column < labelWidth; column++)
                    pixels[((labelY + row) * width) + cellX + column] = Strip;
            for (int character = 0; character < tag.Length; character++)
            {
                if (!PictureTagGlyphs.Rows.TryGetValue(tag[character], out string[]? rows))
                    throw new InvalidDataException($"Tag {tag} holds '{tag[character]}', which the tag font has no glyph for.");
                for (int row = 0; row < PictureTagGlyphs.Height * Scale; row++)
                {
                    for (int column = 0; column < glyph; column++)
                    {
                        if (rows[row / Scale][column / Scale] == '1')
                            pixels[((labelY + 1 + row) * width) + cellX + 1 + (character * (glyph + Scale)) + column] = Text;
                    }
                }
            }
        }
        return (new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)height)), pixels);
    }

    /// <summary><paramref name="source"/> drawn over <paramref name="under"/> by its alpha.</summary>
    private static RgbaPixel Over(RgbaPixel source, RgbaPixel under) => source.A switch
    {
        0 => under,
        255 => source,
        _ => new RgbaPixel(
            (byte)(((source.R * source.A) + (under.R * (255 - source.A))) / 255),
            (byte)(((source.G * source.A) + (under.G * (255 - source.A))) / 255),
            (byte)(((source.B * source.A) + (under.B * (255 - source.A))) / 255)),
    };

    /// <summary>The installation's picture store, read as the files say: its tagged pictures and its decoded pages.</summary>
    private sealed class Store
    {
        /// <summary>Every stored picture's place, by tag.</summary>
        private readonly Dictionary<string, StoredPictureDocument> pictures;

        /// <summary>Each page's pixels and size, decoded when first read.</summary>
        private readonly Lazy<(Extent16<ImagePixels> Size, RgbaPixel[] Pixels)>[] pages;

        /// <summary>Reads the store's index in <paramref name="directory"/>.</summary>
        private Store(string directory, PictureStoreIndex index)
        {
            pictures = index.Pictures ?? throw new InvalidDataException($"Picture store {directory} lists no tagged pictures.");
            pages = [.. Enumerable.Range(0, index.PageCount).Select(page =>
                new Lazy<(Extent16<ImagePixels>, RgbaPixel[])>(() =>
                {
                    using FileStream stream = File.OpenRead(Path.Combine(directory, PictureStoreFiles.PageFile(page)));
                    RgbaPixel[] pixels = RgbaPng.Read(stream, out Extent16<ImagePixels> size);
                    return (size, pixels);
                }))];
        }

        /// <summary>The store of the installation content in <paramref name="content"/>.</summary>
        internal static Store Open(string content)
        {
            string directory = Path.Combine(content, PictureStoreFiles.DirectoryName);
            string path = Path.Combine(directory, PictureStoreFiles.IndexFileName);
            using FileStream stream = File.OpenRead(path);
            return new Store(directory, JsonAssetDocument.Read<PictureStoreIndex>(stream, PictureStoreIndex.JsonOptions,
                $"picture store index {path}"));
        }

        /// <summary>The place of the picture tagged <paramref name="tag"/>, which <paramref name="reader"/> names.</summary>
        /// <exception cref="InvalidDataException">The store does not list the tag.</exception>
        internal StoredPictureDocument Listed(string tag, string reader) => pictures.TryGetValue(tag, out StoredPictureDocument? picture)
            ? picture
            : throw new InvalidDataException($"{reader} names picture tag {tag}, which the picture store does not list.");

        /// <summary>The pixel of <paramref name="picture"/> at (<paramref name="x"/>, <paramref name="y"/>) within it.</summary>
        internal RgbaPixel PixelAt(StoredPictureDocument picture, int x, int y)
        {
            (Extent16<ImagePixels> size, RgbaPixel[] pixels) = pages[picture.Page].Value;
            return pixels[((picture.Corner.Y + y) * size.X) + picture.Corner.X + x];
        }
    }
}
