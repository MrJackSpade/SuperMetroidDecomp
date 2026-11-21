using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Reads an installed sprite sheet's frame manifest and cuts its frames from the sheet image (#1502, #1633, #1635).</summary>
public static class SpriteSheetFile
{
    /// <summary>
    /// The manifest schema revision this reader reads: 13 marks no pixel as lying in a front tile, a picture of a layer's
    /// tiles being installed as its part behind and its part in front instead (#1634); 14 names no colour by a place: its
    /// shades recolour by colour key, its backdrop is a colour key, each frame says whether it blends, and it says which maps
    /// the sheet has (#1633); 15 names the size of the sheet's image and where the picture store holds each frame's picture,
    /// the maps being the store's pages' (#1635); 16 installs no part of a picture that shows nothing, neither as a frame nor
    /// by name, naming as showing nothing only a picture neither part of which shows anything (#1636); 17 names each frame's
    /// picture by its tag as well (#1639); 18 names the recolour-region and blend maps each frame shows its picture with, the
    /// stored picture being its pixels only (#1644); 19 names each frame's picture's size and its place in the frame's
    /// rectangle, the stored picture being the box of what the frame shows (#1646); 20 marks a picture a frame shows repeated
    /// over its rectangle, the stored picture being one period of a turning floor the game repeats beyond its edges (#1651);
    /// 21 names a room block's pictures by the block's number and a colour group's animated shades as its colour animation's
    /// (#1712); 22 names the depth between a room's back and front parts BehindFrontParts and a block animation's frames by
    /// its block animation, not by the hardware's tiles (#1726); 23 names the four Maridia water blends' shades by the tint
    /// each shows instead of a letter (#1740); 24 names the cinematic sprites' frames by their poses, the Ceres floor's Baby
    /// and wing frames by what they show and the Tourian statues' eye glows by their statues, not by numbers (#1747); 25 names the Torizos'
    /// artwork looks for the stage of the eyes, gut, face or chest each shows, not by swap numbers (#1761); 26 names Samus's
    /// projectile, charge-flare and hit-burst frames by the pose each composition shows, not by its place in its family (#1762); 27 names each of the grapple rope's frames by its mirroring, not by the value of its flip bits (#1797).
    /// </summary>
    public const int Version = 27;

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
    /// Cuts a sheet manifest's frames from the sheet's image, of the manifest's size, whose pixels and maps
    /// <paramref name="decode"/> produces when the image is first drawn (#1430).
    /// </summary>
    /// <param name="document">The sheet's manifest: its image's size, frame list and shades.</param>
    /// <param name="name">The sheet's installed name.</param>
    /// <param name="decode">Produces the image's pixels and maps.</param>
    /// <exception cref="InvalidDataException">The manifest names an empty image, or places a frame off the image.</exception>
    public static SpriteSheet Load(InstalledSheetDocument document, string name, Func<SpriteImagePixels> decode)
    {
        Ensure.NotNull(document);
        Ensure.NotNull(decode);
        SpriteImage image;
        try
        {
            image = new SpriteImage(name, document.Size, ColoursOf(document), decode);
        }
        catch (ArgumentException error)
        {
            throw new InvalidDataException($"Sprite sheet {name}: {error.Message}", error);
        }
        return CutFrames(document, name, image);
    }

    /// <summary>Reads and checks a sheet manifest.</summary>
    /// <exception cref="InvalidDataException">The manifest is malformed or of another revision.</exception>
    internal static InstalledSheetDocument ReadDocument(Stream manifest, string name)
    {
        InstalledSheetDocument document = JsonAssetDocument.Read<InstalledSheetDocument>(manifest, Options, $"sprite sheet {name}");
        if (document.Version != Version)
            throw new InvalidDataException($"Sprite sheet {name} has manifest revision {document.Version}, not {Version}.");
        return document;
    }

    /// <summary>The shades the image is pre-coloured in and its scene's backdrop, as a sheet manifest names them by colour key (#1633).</summary>
    /// <exception cref="InvalidDataException">
    /// A shade lists no colours, or a shade or the backdrop names a colour not written as <c>#rrggbb</c>, a negative region,
    /// or one key twice.
    /// </exception>
    internal static PictureColours ColoursOf(InstalledSheetDocument document)
    {
        Ensure.NotNull(document);
        var shades = ImmutableDictionary.CreateBuilder<PictureShade, PictureRecolouring>();
        foreach ((string key, InstalledSheetShadeDocument? shade) in document.Shades ?? [])
        {
            string context = $"Sprite sheet shade {key}";
            if (shade is null || shade.Substitutions is not { } substitutions)
                throw new InvalidDataException($"{context} lists no colours.");
            try
            {
                shades.Add(PictureShade.FromKey(key), new PictureRecolouring([.. substitutions.Select(substitution =>
                    (substitution ?? throw new InvalidDataException($"{context} names a null colour.")).ToSubstitution(context))])
                {
                    Blends = shade.Blends,
                });
            }
            catch (ArgumentException error)
            {
                throw new InvalidDataException($"{context}: {error.Message}", error);
            }
        }
        return new PictureColours
        {
            Backdrop = document.Backdrop?.ToKey("Sprite sheet backdrop"),
            Shades = shades.ToImmutable(),
        };
    }

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
                Blends = document.Blends,
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
