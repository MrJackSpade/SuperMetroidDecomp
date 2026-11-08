using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How the import installs a sheet it composed against the cartridge's colours (#1544): the image gets a list of its own
/// colours, exactly those its pixels show and its scene's backdrop, in the order of the palette indices they were composed
/// as, and each pixel names its colour by its place in that list. Whether a pixel is clear and whether it blends is
/// decided here, from what the cartridge's colour table made of its palette index, and installed with the pixel: Core
/// never infers either from an index.
/// </summary>
internal static class InstalledSheetColouring
{
    /// <summary>Colours in one of the four-colour groups a HUD or room effect sheet is composed in.</summary>
    private const int TwoBppGroupColours = 4;

    /// <summary>
    /// The first of the colour groups whose pixels take part in colour blends where they are drawn as an actor's (#1514): a
    /// composed pixel's group is its palette index over <see cref="ImageColourGroup.Colours"/>, and every later group's
    /// pixels blend too.
    /// </summary>
    private const int FirstBlendingGroup = 4;

    /// <summary>The palette map's green-channel bit marking a pixel that lies in one of its layer's front tiles (#1514).</summary>
    private const int FrontTileMark = 1;

    /// <summary>The palette map's green-channel bit marking a clear pixel.</summary>
    private const int ClearMark = 2;

    /// <summary>The palette map's green-channel bit marking a pixel that blends.</summary>
    private const int BlendsMark = 4;

    /// <summary>
    /// The installed manifest and each pixel's ink of a sheet composed as <paramref name="composed"/>, whose pixels were
    /// composed as <paramref name="paletteIndices"/> and marked as lying in front tiles by <paramref name="frontTiles"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The composed manifest names the wrong number of colours, a pixel or the backdrop names a palette index past them, or
    /// a front-tile mark is neither zero nor one.
    /// </exception>
    internal static InstalledSheetLayers Install(SpriteSheetDocument composed, ReadOnlySpan<byte> paletteIndices, ReadOnlySpan<byte> frontTiles)
    {
        Ensure.NotNull(composed);
        Ensure.LengthEqual(frontTiles, paletteIndices.Length);
        int tableColours = ImageColourGroup.ColourCountOf(composed.Palette);
        if (composed.Colours is null || composed.Colours.Length != tableColours)
            throw new InvalidDataException($"A {composed.Palette} sheet names {composed.Colours?.Length ?? 0} colours, not {tableColours}.");
        var used = new bool[tableColours];
        for (int pixel = 0; pixel < paletteIndices.Length; pixel++)
        {
            int index = paletteIndices[pixel];
            if (index >= tableColours)
                throw new InvalidDataException($"A {composed.Palette} sheet's pixel names palette index {index}, past its {tableColours} colours.");
            if (!IsTransparent(composed.Palette, index))
                used[index] = true;
        }
        if (composed.Backdrop is { } backdrop)
        {
            if (backdrop < 0 || backdrop >= tableColours)
                throw new InvalidDataException($"A {composed.Palette} sheet names backdrop palette index {backdrop}, past its {tableColours} colours.");
            used[backdrop] = true;
        }
        var placeOf = new int[tableColours];
        var sources = new List<int>();
        for (int index = 0; index < tableColours; index++)
        {
            placeOf[index] = used[index] ? sources.Count : -1;
            if (used[index])
                sources.Add(index);
        }
        var inks = new SpriteImageInk[paletteIndices.Length];
        for (int pixel = 0; pixel < inks.Length; pixel++)
        {
            int front = frontTiles[pixel];
            if (front > 1)
                throw new InvalidDataException($"A sheet marks a pixel's front tile {front}; a pixel lies in a front tile (one) or not (zero).");
            int index = paletteIndices[pixel];
            inks[pixel] = ShownKindOf(composed.Palette, index) is { } kind ? new SpriteImageInk(placeOf[index], kind, front != 0)
                : SpriteImageInk.Transparent(front != 0);
        }
        var manifest = new InstalledSheetDocument
        {
            Version = SpriteSheetFile.Version,
            Colours = [.. sources.Select(index => composed.Colours[index])],
            Backdrop = composed.Backdrop is { } shown ? placeOf[shown] : null,
            Shades = composed.Shades?.ToDictionary(shade => shade.Key, shade => Installed(shade.Key, shade.Value, placeOf), StringComparer.Ordinal),
            Frames = composed.Frames.ToDictionary(frame => frame.Key, frame => Installed(frame.Value), StringComparer.Ordinal),
            Blank = composed.Blank,
            Source = new InstalledSheetSource
            {
                Palette = composed.Palette,
                PaletteBits = composed.PaletteBits,
                BackgroundSets = composed.BackgroundSets,
                Inks = [.. sources],
            },
        };
        return new InstalledSheetLayers(manifest, inks);
    }

    /// <summary>The palette map pixel that installs <paramref name="ink"/>: its colour's place in red, its marks in green, opaque where it has a colour.</summary>
    internal static RgbaPixel MapPixelOf(SpriteImageInk ink) => new(
        checked((byte)(ink.Colour ?? 0)),
        (byte)((ink.FrontTile ? FrontTileMark : 0) | (ink.Clear ? ClearMark : 0) | (ink.Blends ? BlendsMark : 0)),
        0,
        ink.IsTransparent ? (byte)0 : byte.MaxValue);

    /// <summary>The ink a palette map pixel installs: transparent where the map pixel is, otherwise the colour its red names.</summary>
    internal static SpriteImageInk InkOf(RgbaPixel mapPixel)
    {
        bool front = (mapPixel.G & FrontTileMark) != 0;
        return mapPixel.A == 0 ? SpriteImageInk.Transparent(front)
            : new SpriteImageInk(mapPixel.R, MarkedKind(mapPixel.G), front);
    }

    /// <summary>
    /// How a pixel of a <paramref name="palette"/> sheet composed as <paramref name="index"/> shows its colour once installed,
    /// or none where it is transparent and has no colour: what the installed pixel's marks will say of it.
    /// </summary>
    internal static ColouredInkKind? ShownKindOf(SpriteImageColours palette, int index) =>
        IsTransparent(palette, index) ? null : KindOf(palette, index);

    /// <summary>How a coloured pixel composed as <paramref name="index"/> shows its colour.</summary>
    private static ColouredInkKind KindOf(SpriteImageColours palette, int index) =>
        IsClear(palette, index) ? ColouredInkKind.Clear
        : Blends(index) ? ColouredInkKind.Blending
        : ColouredInkKind.Opaque;

    /// <summary>How a coloured pixel whose map marks are <paramref name="marks"/> shows its colour.</summary>
    /// <exception cref="InvalidDataException">The marks make the pixel both clear and blending.</exception>
    private static ColouredInkKind MarkedKind(int marks)
    {
        bool clear = (marks & ClearMark) != 0;
        bool blends = (marks & BlendsMark) != 0;
        if (clear && blends)
            throw new InvalidDataException("A palette map pixel marks a pixel both clear and blending.");
        return clear ? ColouredInkKind.Clear : blends ? ColouredInkKind.Blending : ColouredInkKind.Opaque;
    }

    /// <summary>
    /// Whether a pixel composed as <paramref name="index"/> is transparent: colour zero, which the sheet leaves clear with
    /// no colour, except on a HUD or room effect sheet, whose colour-zero pixels keep their group's colour zero.
    /// </summary>
    private static bool IsTransparent(SpriteImageColours palette, int index) => palette != SpriteImageColours.TwoBppBackground && index == 0;

    /// <summary>
    /// Whether a pixel composed as <paramref name="index"/> is clear: colour zero of its group on a HUD or room effect sheet,
    /// which shows what lies behind it unless its layer is drawn opaque and then its own colour (#1408).
    /// </summary>
    private static bool IsClear(SpriteImageColours palette, int index) =>
        palette == SpriteImageColours.TwoBppBackground && index % TwoBppGroupColours == 0;

    /// <summary>Whether a pixel composed as <paramref name="index"/> blends where it is drawn as an actor's: its group is one that blends.</summary>
    private static bool Blends(int index) => index / ImageColourGroup.Colours >= FirstBlendingGroup;

    /// <summary>A composed shade as the installed image's colours name it: each recoloured palette index the image has, by its place.</summary>
    private static SpriteSheetShadeDocument Installed(string key, SpriteSheetShadeDocument shade, int[] placeOf)
    {
        if (shade.Inks.Length != shade.Colours.Length)
            throw new InvalidDataException($"Shade {key} recolours {shade.Inks.Length} inks with {shade.Colours.Length} colours.");
        var inks = new List<int>();
        var colours = new List<ushort>();
        for (int entry = 0; entry < shade.Inks.Length; entry++)
        {
            int index = shade.Inks[entry];
            if (index < 0 || index >= placeOf.Length)
                throw new InvalidDataException($"Shade {key} recolours palette index {index}, past its sheet's {placeOf.Length} colours.");
            // A colour no pixel shows is not one of the image's, and recolouring it shows nothing.
            if (placeOf[index] < 0)
                continue;
            inks.Add(placeOf[index]);
            colours.Add(shade.Colours[entry]);
        }
        return new SpriteSheetShadeDocument { Inks = [.. inks], Colours = [.. colours], ColourMath = shade.ColourMath };
    }

    /// <summary>A composed frame as the installed manifest keeps it.</summary>
    private static InstalledSheetFrameDocument Installed(SpriteSheetFrameDocument frame) =>
        new() { Corner = frame.Corner, Size = frame.Size, Origin = frame.Origin, Depth = frame.Depth };
}
