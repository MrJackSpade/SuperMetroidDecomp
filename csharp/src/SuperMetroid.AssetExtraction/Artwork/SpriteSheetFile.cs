using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Reads an installed sprite sheet's frame manifest and cuts its frames from the sheet image (#1502).</summary>
public static class SpriteSheetFile
{
    /// <summary>The manifest schema revision this reader reads.</summary>
    public const int Version = 9;

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

    /// <summary>Reads a sheet manifest and cuts its frames from the sheet image.</summary>
    /// <param name="manifest">The frame list and standard colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="pixels">The image's pixels.</param>
    /// <param name="paletteIndices">Each pixel's colour group's place times sixteen plus its colour.</param>
    /// <param name="priorities">Each pixel's priority against the backgrounds 0..3.</param>
    /// <exception cref="InvalidDataException">The manifest is malformed, of another revision, or places a frame off the image.</exception>
    public static SpriteSheet Load(Stream manifest, string name, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> priorities)
    {
        SpriteSheetDocument document = ReadDocument(manifest, name);
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, size, pixels, paletteIndices, PaletteOf(document), priorities);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        InstalledPictureColours.Record(image, [.. document.BackgroundSets ?? []], document.PaletteBits);
        return CutFrames(document, name, image);
    }

    /// <summary>
    /// Reads a sheet manifest and cuts its frames from a sheet image whose layers <paramref name="decode"/> produces
    /// when the image is first drawn (#1430).
    /// </summary>
    /// <param name="manifest">The frame list and standard colours.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="size">The image's width and height.</param>
    /// <param name="decode">Produces the image's pixels, palette indices and priorities.</param>
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
        InstalledPictureColours.Record(image, [.. document.BackgroundSets ?? []], document.PaletteBits);
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
        new(document.Palette, [.. (document.Colours ?? []).Select(RgbColor.FromWord)])
        {
            Shades = (document.Shades ?? []).ToImmutableDictionary(
                shade => PictureShade.FromKey(shade.Key),
                shade => new ShadeInks([.. shade.Value.Inks], [.. shade.Value.Colours.Select(RgbColor.FromWord)])
                {
                    TakesColourMath = shade.Value.ColourMath,
                }),
        };

    /// <summary>Cuts a manifest's frames from the sheet image.</summary>
    /// <exception cref="InvalidDataException">The manifest lists no frames, a null frame, or places a frame off the image.</exception>
    private static SpriteSheet CutFrames(SpriteSheetDocument document, string name, SpriteImage image)
    {
        var frames = new Dictionary<string, SpriteSheetFrame>(StringComparer.Ordinal);
        foreach ((string frameName, SpriteSheetFrameDocument? frame) in document.Frames ??
            throw new InvalidDataException($"Sprite sheet {name} has no frame list."))
        {
            frames.Add(frameName, Cut(image, name, frameName, frame
                ?? throw new InvalidDataException($"Sprite sheet {name} frame {frameName} is null.")));
        }
        return new SpriteSheet(image, frames);
    }

    /// <summary>Cuts one frame from the sheet image.</summary>
    private static SpriteSheetFrame Cut(SpriteImage image, string sheet, string name, SpriteSheetFrameDocument document)
    {
        try
        {
            return new SpriteSheetFrame(image, document.Corner, document.Size, document.Origin, ImageColourGroups.FromManifest(document.ColourGroups));
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {sheet} frame {name}: {error.Message}", error);
        }
    }

    /// <summary>The frame set the import checks a loaded sheet against.</summary>
    extension(SpriteSheet sheet)
    {
        /// <summary>The names of the sheet's frames.</summary>
        internal IEnumerable<string> FrameNames => sheet.frames.Keys;

        /// <summary>Whether the sheet holds exactly the frames named <paramref name="names"/>.</summary>
        public bool HoldsExactly(IEnumerable<string> names) => sheet.FrameNames.ToHashSet(StringComparer.Ordinal).SetEquals(names);
    }
}
