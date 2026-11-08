using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Reads an installed sprite sheet's frame manifest and cuts its frames from the sheet image (#1502).</summary>
public static class SpriteSheetFile
{
    /// <summary>The manifest schema revision this reader reads.</summary>
    public const int Version = 11;

    /// <summary>
    /// Camel-case manifest fields; unknown fields are rejected. The import writes manifests with the same settings, unindented:
    /// sheet manifests run to hundreds of megabytes across an installation, and indenting them more than doubles what the
    /// install writes and every load parses (#1530).
    /// </summary>
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
    };

    /// <summary>
    /// Reads a sheet manifest and cuts its frames from a sheet image whose layers <paramref name="decode"/> produces
    /// when the image is first drawn (#1430).
    /// </summary>
    /// <param name="manifest">The frame list and the image's own colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="decode">Produces the image's pixels and inks.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, Func<SpriteImageLayers> decode)
    {
        InstalledSheetDocument document = ReadDocument(manifest, name);
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, size, ColoursOf(document), decode);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        InstalledPictureColours.Record(image, document.Source);
        return CutFrames(document, name, image);
    }

    /// <summary>Reads and checks a sheet manifest.</summary>
    /// <exception cref="InvalidDataException">
    /// The manifest is malformed or of another revision, or does not say where each of its colours came from.
    /// </exception>
    internal static InstalledSheetDocument ReadDocument(Stream manifest, string name)
    {
        InstalledSheetDocument document = JsonAssetDocument.Read<InstalledSheetDocument>(manifest, Options, $"sprite sheet {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Sprite sheet {name} has manifest revision {document.Version}, not {Version}.");
        if (document.Colours is null || document.Source?.Inks is null || document.Source.Inks.Length != document.Colours.Length)
            throw new InvalidDataException($"Sprite sheet {name} does not say where each of its {document.Colours?.Length ?? 0} colours came from.");
        return document;
    }

    /// <summary>The image's own colours, its shades and its scene's backdrop, as a sheet manifest names them.</summary>
    internal static PictureColours ColoursOf(InstalledSheetDocument document) =>
        new([.. Ensure.NotNull(document).Colours.Select(RgbColor.FromWord)])
        {
            Backdrop = document.Backdrop,
            Shades = (document.Shades ?? []).ToImmutableDictionary(
                shade => PictureShade.FromKey(shade.Key),
                shade => new ShadeInks([.. shade.Value.Inks], [.. shade.Value.Colours.Select(RgbColor.FromWord)])
                {
                    Blends = shade.Value.ColourMath,
                }),
        };

    /// <summary>Cuts a manifest's frames from the sheet image and keeps the names of the frames that show nothing.</summary>
    /// <exception cref="InvalidDataException">
    /// The manifest lists no frames, a null frame, places a frame off the image, or names a frame that shows nothing twice,
    /// as null, or as a pictured frame too.
    /// </exception>
    internal static SpriteSheet CutFrames(InstalledSheetDocument document, string name, SpriteImage image)
    {
        var frames = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string frameName, InstalledSheetFrameDocument? frame) in document.Frames ??
            throw new InvalidDataException($"Sprite sheet {name} has no frame list."))
        {
            frames.Add(frameName, Cut(image, name, frameName, frame
                ?? throw new InvalidDataException($"Sprite sheet {name} frame {frameName} is null.")));
        }
        var blanks = new HashSet<string>(StringComparer.Ordinal);
        foreach (string? blank in document.Blank ?? [])
        {
            if (blank is null)
                throw new InvalidDataException($"Sprite sheet {name} names a null frame that shows nothing.");
            if (frames.ContainsKey(blank))
                throw new InvalidDataException($"Sprite sheet {name} frame {blank} is pictured and shows nothing.");
            if (!blanks.Add(blank))
                throw new InvalidDataException($"Sprite sheet {name} names frame {blank} as showing nothing twice.");
        }
        return new SpriteSheet(image, frames, blanks);
    }

    /// <summary>Cuts one frame from the sheet image.</summary>
    private static SpriteSheetFrame Cut(SpriteImage image, string sheet, string name, InstalledSheetFrameDocument document)
    {
        try
        {
            return new SpriteSheetFrame(image, document.Corner, document.Size, document.Origin)
            {
                Depth = document.Depth,
            };
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {sheet} frame {name}: {error.Message}", error);
        }
    }

    /// <summary>The frame set the import checks a loaded sheet against.</summary>
    extension(SpriteSheet sheet)
    {
        /// <summary>The names of the sheet's pictured frames.</summary>
        internal IEnumerable<string> FrameNames => sheet.frames.Keys;

        /// <summary>The names of the frames the sheet holds as showing nothing (#1513).</summary>
        internal IEnumerable<string> BlankFrameNames => sheet.blanks;

        /// <summary>
        /// Whether the sheet holds exactly the frames named <paramref name="names"/>, each either pictured or as one that
        /// shows nothing.
        /// </summary>
        public bool HoldsExactly(IEnumerable<string> names) =>
            sheet.FrameNames.Concat(sheet.BlankFrameNames).ToHashSet(StringComparer.Ordinal).SetEquals(names);
    }
}
