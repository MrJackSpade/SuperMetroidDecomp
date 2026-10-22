using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>An installed sprite sheet: one pre-coloured image and the named frames cut from it.</summary>
public sealed class SpriteSheet
{
    /// <summary>The manifest schema revision this catalog reads.</summary>
    public const int Version = 1;

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
        content.Append("width", Image.Size.X);
        content.Append("height", Image.Size.Y);
        content.Append("pixels", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.Pixels));
        content.Append("palette indices", Image.PaletteIndices);
        content.Append("standard colours", System.Runtime.InteropServices.MemoryMarshal.AsBytes(Image.StandardColours));
        content.Append("priorities", Image.Priorities);
        content.Append("part rows", Image.PartRows);
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
                content.Append("row", (frame.Rows & row) != 0 ? 1 : 0);
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
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, ReadOnlySpan<Rgba32> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> priorities, ReadOnlySpan<byte> partRows)
    {
        SpriteSheetDocument document = JsonAssetDocument.Read<SpriteSheetDocument>(manifest, Options, $"sprite sheet {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Sprite sheet {name} has manifest revision {document.Version}, not {Version}.");
        if (document.Colours is not { Length: SpriteImage.SpriteColourCount })
            throw new InvalidDataException($"Sprite sheet {name} needs its {SpriteImage.SpriteColourCount} standard colours.");
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, size, pixels, paletteIndices, [.. document.Colours.Select(Bgr555.FromWord)], priorities, partRows);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        var frames = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string frameName, SpriteSheetFrameDocument? frame) in document.Frames ??
            throw new InvalidDataException($"Sprite sheet {name} has no frame list."))
        {
            if (frame is null)
                throw new InvalidDataException($"Sprite sheet {name} frame {frameName} is null.");
            try
            {
                frames.Add(frameName, new SpriteSheetFrame(image, frame.Corner, frame.Size, frame.Origin, frame.Rows, frame.Objects));
            }
            catch (ArgumentException error)
            {
                throw new InvalidDataException($"Sprite sheet {name} frame {frameName}: {error.Message}", error);
            }
        }
        return new SpriteSheet(image, frames);
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
