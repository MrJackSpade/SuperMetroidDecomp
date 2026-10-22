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
        foreach ((string name, SpriteSheetFrame frame) in frames.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            content.AppendName("frame", name);
            content.Append("corner x", frame.Corner.X);
            content.Append("corner y", frame.Corner.Y);
            content.Append("width", frame.Size.X);
            content.Append("height", frame.Size.Y);
            content.Append("origin x", frame.Origin.X);
            content.Append("origin y", frame.Origin.Y);
            content.Append("blends", frame.Blends ? 1 : 0);
            content.Append("priority", frame.Priority);
        }
    }

    /// <summary>Reads a sheet manifest and cuts its frames from <paramref name="image"/>.</summary>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, SpriteImage image)
    {
        Ensure.NotNull(image);
        SpriteSheetDocument document = JsonAssetDocument.Read<SpriteSheetDocument>(manifest, Options, $"sprite sheet {image.Name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Sprite sheet {image.Name} has manifest revision {document.Version}, not {Version}.");
        var frames = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string name, SpriteSheetFrameDocument? frame) in document.Frames ??
            throw new InvalidDataException($"Sprite sheet {image.Name} has no frame list."))
        {
            if (frame is null)
                throw new InvalidDataException($"Sprite sheet {image.Name} frame {name} is null.");
            try
            {
                frames.Add(name, new SpriteSheetFrame(image, frame.Corner, frame.Size, frame.Origin, frame.Blends, frame.Priority));
            }
            catch (ArgumentException error)
            {
                throw new InvalidDataException($"Sprite sheet {image.Name} frame {name}: {error.Message}", error);
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
