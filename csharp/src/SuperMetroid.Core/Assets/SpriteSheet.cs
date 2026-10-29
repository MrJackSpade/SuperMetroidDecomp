using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>An installed sprite sheet: one pre-coloured image and the named frames cut from it.</summary>
public sealed class SpriteSheet
{
    /// <summary>The manifest schema revision this catalog reads.</summary>
    public const int Version = 4;

    /// <summary>The frames by name.</summary>
    private readonly Dictionary<string, SpriteSheetFrame> frames;

    /// <summary>Keeps the validated frames.</summary>
    private SpriteSheet(SpriteImage image, Dictionary<string, SpriteSheetFrame> frames)
    {
        Image = image;
        this.frames = frames;
    }

    /// <summary>The sheet image.</summary>
    public SpriteImage Image { get; }

    /// <summary>The frame named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    public SpriteSheetFrame Frame(string name) =>
        frames.TryGetValue(name, out SpriteSheetFrame? frame) ? frame : throw new InvalidDataException($"Sprite sheet {Image.Name} has no frame {name}.");

    /// <summary>Whether the sheet holds exactly the frames named <paramref name="names"/>.</summary>
    public bool HoldsExactly(IEnumerable<string> names) => frames.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(names);

    /// <summary>Appends the sheet's pixels and frames to a content identity, frames in name order.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.AppendName("sheet", Image.Name);
        content.AppendName("palette", Image.Colours.ToString());
        foreach (BackgroundColourSet set in Image.BackgroundSets)
            content.AppendName("background set", set.ToString());
        content.Append("width", Image.Size.X);
        content.Append("height", Image.Size.Y);
        content.Append("pixels", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.Pixels));
        // Each ink field is identified as its own plane, as the image was installed.
        ReadOnlySpan<SpriteImageInk> inks = Image.Inks;
        content.Append("palette indices", inks.SelectToArray(ink => (byte)ink.PaletteIndex));
        content.Append("standard colours", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.StandardColours));
        content.Append("priorities", inks.SelectToArray(ink => (byte)ink.Priority));
        content.Append("part rows", inks.SelectToArray(ink => (byte)ink.PartRow));
        foreach ((string name, SpriteSheetFrame frame) in frames.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            content.AppendName("frame", name);
            content.Append("corner x", frame.Corner.X);
            content.Append("corner y", frame.Corner.Y);
            content.Append("width", frame.Size.X);
            content.Append("height", frame.Size.Y);
            content.Append("origin x", frame.Origin.X);
            content.Append("origin y", frame.Origin.Y);
            foreach (SpritePaletteRows row in SpritePaletteRowSet.Rows)
                content.Append("row", (frame.Rows & row) != SpritePaletteRows.None ? 1 : 0);
            foreach (SpriteSheetFramePart part in frame.Parts)
            {
                content.Append("part corner x", part.Image.Corner.X);
                content.Append("part corner y", part.Image.Corner.Y);
                content.Append("part width", part.Image.Size.X);
                content.Append("part height", part.Image.Size.Y);
                content.Append("part offset x", part.Offset.X);
                content.Append("part offset y", part.Offset.Y);
            }
        }
    }

    /// <summary>Reads a sheet manifest and cuts its frames from the sheet image.</summary>
    /// <param name="manifest">The frame list and standard colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="pixels">The image's pixels.</param>
    /// <param name="paletteIndices">Each pixel's palette row times sixteen plus its colour.</param>
    /// <param name="priorities">Each pixel's priority against the backgrounds 0..3.</param>
    /// <param name="partRows">Each pixel's part palette row 0..7.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> priorities, ReadOnlySpan<byte> partRows)
    {
        SpriteSheetDocument document = ReadDocument(manifest, name);
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, size, pixels, paletteIndices, PaletteOf(document), priorities, partRows);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        return CutFrames(document, name, image);
    }

    /// <summary>
    /// Reads a sheet manifest and cuts its frames from a sheet image whose layers <paramref name="decode"/> produces
    /// when the image is first drawn (#1430).
    /// </summary>
    /// <param name="manifest">The frame list and standard colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="decode">Produces the image's pixels, palette indices, priorities and part rows.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, Func<SpriteImageLayers> decode)
    {
        SpriteSheetDocument document = ReadDocument(manifest, name);
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, size, PaletteOf(document), decode);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        return CutFrames(document, name, image);
    }

    /// <summary>Reads and checks a sheet manifest.</summary>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or lacks its standard colours.</exception>
    private static SpriteSheetDocument ReadDocument(Stream manifest, string name)
    {
        SpriteSheetDocument document = JsonAssetDocument.Read<SpriteSheetDocument>(manifest, Options, $"sprite sheet {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Sprite sheet {name} has manifest revision {document.Version}, not {Version}.");
        if (document.Colours is null || document.Colours.Length != SpriteImage.ColourCountOf(document.Palette))
            throw new InvalidDataException($"Sprite sheet {name} needs its {SpriteImage.ColourCountOf(document.Palette)} standard colours.");
        return document;
    }

    /// <summary>The standard colours a sheet manifest names.</summary>
    private static SpriteImagePalette PaletteOf(SpriteSheetDocument document) =>
        new(document.Palette, [.. (document.Colours ?? []).Select(RgbColor.FromWord)]) { BackgroundSets = [.. document.BackgroundSets ?? []] };

    /// <summary>Cuts a manifest's part images and frames from the sheet image.</summary>
    /// <exception cref="InvalidDataException">The manifest places a frame off the image or names a missing part image.</exception>
    private static SpriteSheet CutFrames(SpriteSheetDocument document, string name, SpriteImage image)
    {
        // Part images come first: the frames name them.
        var partImages = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string partName, SpriteSheetFrameDocument? part) in document.PartImages ??
            throw new InvalidDataException($"Sprite sheet {name} has no part image list."))
        {
            if (part is not { Parts.Length: 0 })
                throw new InvalidDataException($"Sprite sheet {name} part image {partName} is null or has parts of its own.");
            partImages.Add(partName, Cut(image, name, partName, part, []));
        }
        var frames = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string frameName, SpriteSheetFrameDocument? frame) in document.Frames ??
            throw new InvalidDataException($"Sprite sheet {name} has no frame list."))
        {
            if (frame?.Parts is null)
                throw new InvalidDataException($"Sprite sheet {name} frame {frameName} is null or has no part list.");
            SpriteSheetFramePart[] parts = [.. frame.Parts.Select(part =>
            {
                SpriteSheetFrame partImage = partImages.TryGetValue(part.Image, out SpriteSheetFrame? found) ? found
                    : throw new InvalidDataException($"Sprite sheet {name} frame {frameName} names missing part image {part.Image}.");
                // The part lands at its place from the frame's origin when drawn at that origin.
                var placed = new SpriteSheetFrame(image, partImage.Corner, partImage.Size, partImage.Origin + -part.Offset, partImage.Rows, []);
                return new SpriteSheetFramePart(placed, part.Offset);
            })];
            frames.Add(frameName, Cut(image, name, frameName, frame, parts));
        }
        return new SpriteSheet(image, frames);
    }

    /// <summary>Cuts one frame or part image from the sheet image.</summary>
    private static SpriteSheetFrame Cut(SpriteImage image, string sheet, string name, SpriteSheetFrameDocument document,
        IReadOnlyList<SpriteSheetFramePart> parts)
    {
        try
        {
            return new SpriteSheetFrame(image, document.Corner, document.Size, document.Origin, document.Rows, parts);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {sheet} frame {name}: {error.Message}", error);
        }
    }

    /// <summary>Serializes a sheet manifest.</summary>
    public static byte[] Write(SpriteSheetDocument document) => JsonSerializer.SerializeToUtf8Bytes(Ensure.NotNull(document), Options);

    /// <summary>Camel-case manifest fields; unknown fields are rejected.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
