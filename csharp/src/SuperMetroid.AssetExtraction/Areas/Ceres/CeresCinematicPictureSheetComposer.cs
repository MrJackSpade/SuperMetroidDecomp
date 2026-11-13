using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Composes the Ceres cinematics' background pictures once (#1467): each SPACE COLONY caption letter and the katakana
/// caption over the flight's object characters, and the Zebes planet over the characters the destruction leaves once the
/// reveal loads its own, all under the flight's palette, each installed whole (#1634).
/// </summary>
internal static class CeresCinematicPictureSheetComposer
{
    /// <summary>The katakana caption's width in characters: an eight-by-two block below SPACE COLONY.</summary>
    internal const int JapaneseWidth = 8;

    /// <summary>The name of every picture the sheet holds.</summary>
    internal static IEnumerable<string> FrameNames =>
        [.. Enumerable.Range(0, SpaceColonyCaptionDefinitions.LetterCount).Select(CeresCinematicPictures.CaptionLetter), CeresCinematicPictures.JapaneseCaption, CeresCinematicPictures.ZebesPlanet];

    /// <summary>The caption character visible letter <paramref name="index"/> draws.</summary>
    internal static char CaptionCharacter(int index) => SpaceColonyCaptionDefinitions.Text[SpaceColonyCaption.TextIndex(index)];

    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>The cells along each side of the Zebes planet's map.</summary>
    private const int PlanetCells = 32;

    /// <summary>
    /// Composes the sheet from the installed intro artwork. Every picture shows its own colours (#1544): the gunship
    /// engine's flicker shades only the gunship's floor.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture's tile names a character the scene never loads.</exception>
    internal static ComposedSpriteSheet Compose(ImportIntroCinematicArtwork intro) =>
        Compose(Ensure.NotNull(intro).FlightCharacters, intro);

    /// <summary>
    /// Composes the sheet from the flight artwork <paramref name="ceresFlight"/> and the rest of <paramref name="intro"/>'s
    /// artwork, as an installation whose flight artwork is replaced composes it.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture's tile names a character the scene never loads.</exception>
    internal static ComposedSpriteSheet Compose(CeresFlightCharacters ceresFlight, ImportIntroCinematicArtwork intro)
    {
        Ensure.NotNull(ceresFlight);
        Ensure.NotNull(intro);
        byte[] flight = ceresFlight.ObjectCharacters.Span[..CeresFlightArtworkFileFormat.ObjectByteCount].ToArray();
        // The destruction loads the shared cinematic sheet over the start of the flight's objects; the reveal its own over them.
        byte[] zebes = (byte[])flight.Clone();
        intro.IntroObjectCharacters.Characters.Span[..CeresFlightArtworkFileFormat.SharedObjectCharacterBytes].CopyTo(zebes);
        intro.ZebesReveal.Characters.Characters.Span[..ZebesRevealArtworkFormat.CharacterByteCount].CopyTo(zebes);
        int letters = SpaceColonyCaptionDefinitions.LetterCount;
        // Each caption letter's glyph is pictured once, however many of the caption's letters draw it (#1513).
        BackgroundTile[] glyphs = [.. Enumerable.Range(0, letters).Select(letter => Ensure.Listed(SpaceColonyCaptionTileDefinitions.Glyphs, CaptionCharacter(letter))).Distinct()];
        int japaneseRows = SpaceColonyCaptionTileDefinitions.JapaneseTiles.Length / JapaneseWidth;
        int captionWidth = (glyphs.Length + JapaneseWidth) * CharacterPixels;
        int width = Math.Max(captionWidth, PlanetCells * CharacterPixels);
        int captionHeight = japaneseRows * CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)(captionHeight + PlanetCells * CharacterPixels))));
        RgbColor[] colours = ScreenPaletteColours.FlightColours(intro)[..ImageColourGroup.HalfColours].ToArray();
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        // The caption's glyphs side by side, then the katakana caption beside them, then the planet below.
        var glyphGroups = new ImageColourGroups[glyphs.Length];
        for (int glyph = 0; glyph < glyphs.Length; glyph++)
            glyphGroups[glyph] = Paint(canvas, glyph * CharacterPixels, 0, glyphs[glyph], flight, colours);
        for (int letter = 0; letter < letters; letter++)
        {
            int glyph = Array.IndexOf(glyphs, Ensure.Listed(SpaceColonyCaptionTileDefinitions.Glyphs, CaptionCharacter(letter)));
            frames.Add(CeresCinematicPictures.CaptionLetter(letter), Frame(glyph * CharacterPixels, 0, 1, 1, glyphGroups[glyph]));
        }
        ImageColourGroups japanese = ImageColourGroups.None;
        ReadOnlySpan<BackgroundTile> japaneseTiles = SpaceColonyCaptionTileDefinitions.JapaneseTiles;
        for (int cell = 0; cell < japaneseTiles.Length; cell++)
            japanese = japanese.Union(Paint(canvas, (glyphs.Length + cell % JapaneseWidth) * CharacterPixels,
                cell / JapaneseWidth * CharacterPixels, japaneseTiles[cell], flight, colours));
        frames.Add(CeresCinematicPictures.JapaneseCaption, Frame(glyphs.Length * CharacterPixels, 0, JapaneseWidth, japaneseRows, japanese));
        ReadOnlySpan<BackgroundTile> planet = intro.ZebesReveal.Map.Tiles.Span[..(ZebesRevealArtworkFormat.MapByteCount / sizeof(ushort))];
        ImageColourGroups planetGroups = ImageColourGroups.None;
        for (int cell = 0; cell < planet.Length; cell++)
            planetGroups = planetGroups.Union(Paint(canvas, cell % PlanetCells * CharacterPixels, captionHeight + cell / PlanetCells * CharacterPixels,
                planet[cell], zebes, colours));
        frames.Add(CeresCinematicPictures.ZebesPlanet, Frame(0, captionHeight, PlanetCells, planet.Length / PlanetCells, planetGroups));
        return canvas.ToSheet(colours, frames, SpriteImageColours.Background);
    }

    /// <summary>Paints <paramref name="tile"/> at (<paramref name="left"/>, <paramref name="top"/>) from <paramref name="characters"/>.</summary>
    /// <exception cref="InvalidDataException">The tile names a character past those loaded.</exception>
    private static ImageColourGroups Paint(RoomTileCanvas canvas, int left, int top, BackgroundTile tile, byte[] characters, RgbColor[] colours)
    {
        int start = tile.Tile * RoomTileCanvas.CharacterBytes;
        if (start + RoomTileCanvas.CharacterBytes > characters.Length)
            throw new InvalidDataException($"A Ceres cinematic picture names character {tile.Tile}, which the scene never loads.");
        return canvas.PaintTile(left, top, tile, characters.AsSpan(start, RoomTileCanvas.CharacterBytes), colours);
    }

    /// <summary>The frame of the <paramref name="columns"/> by <paramref name="rows"/> cells from pixel (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private static SpriteSheetFrameDocument Frame(int left, int top, int columns, int rows, ImageColourGroups coloured) => new()
    {
        Corner = new Position16<ImagePixels>(checked((ushort)left), checked((ushort)top)),
        Size = new Extent16<ImagePixels>(checked((ushort)(columns * CharacterPixels)), checked((ushort)(rows * CharacterPixels))),
        Origin = default,
        ColourGroups = coloured.ManifestMembers,
    };
}
