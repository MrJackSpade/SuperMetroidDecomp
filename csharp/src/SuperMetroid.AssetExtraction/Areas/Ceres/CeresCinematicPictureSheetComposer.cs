using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// Composes the Ceres cinematics' background pictures once (#1467): each SPACE COLONY caption letter and the katakana
/// caption over the flight's object characters, and the Zebes planet over the characters the destruction leaves once the
/// reveal loads its own, all under the flight's palette, each pixel in the priority tier its tile gave it.
/// </summary>
internal static class CeresCinematicPictureSheetComposer
{
    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>The cells along each side of the Zebes planet's map.</summary>
    private const int PlanetCells = 32;

    /// <summary>Composes the sheet from the installed intro artwork.</summary>
    /// <exception cref="InvalidDataException">A picture's tile names a character the scene never loads.</exception>
    internal static ComposedSpriteSheet Compose(IntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        byte[] flight = CeresFlightCharacters.Of(intro.CeresFlight).ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount].ToArray();
        // The destruction loads the shared cinematic sheet over the start of the flight's objects; the reveal its own over them.
        byte[] zebes = (byte[])flight.Clone();
        IntroCinematicSourceArtwork.Of(intro).IntroObjectCharacters.Characters.Span[..CeresDestructionSceneDefinitions.SharedObjectCharacterBytes].CopyTo(zebes);
        intro.CeresDestruction.ZebesCharacters.Characters.Span[..CeresDestructionArtworkFormat.ZebesCharacterByteCount].CopyTo(zebes);
        int letters = SpaceColonyCaptionDefinitions.LetterCount;
        int japaneseRows = SpaceColonyCaptionTileDefinitions.JapaneseTiles.Length / SpaceColonyCaptionDefinitions.JapaneseWidth;
        int captionWidth = (letters + SpaceColonyCaptionDefinitions.JapaneseWidth) * CharacterPixels;
        int width = Math.Max(captionWidth, PlanetCells * CharacterPixels);
        int captionHeight = japaneseRows * CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)(captionHeight + PlanetCells * CharacterPixels))));
        RgbColor[] colours = ScreenTileSheetComposer.FlightColours(intro.CeresFlight)[..SpriteImage.SpriteColourCount].ToArray();
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        // The caption letters side by side, then the katakana caption beside them, then the planet below.
        for (int letter = 0; letter < letters; letter++)
        {
            BackgroundTile glyph = Ensure.Listed(SpaceColonyCaptionTileDefinitions.Glyphs, SpaceColonyCaption.Character(letter));
            frames.Add(CeresCinematicPictures.CaptionLetter(letter), Frame(letter * CharacterPixels, 0, 1, 1,
                Paint(canvas, letter * CharacterPixels, 0, glyph, flight, colours)));
        }
        ImageColourGroups japanese = ImageColourGroups.None;
        ReadOnlySpan<BackgroundTile> japaneseTiles = SpaceColonyCaptionTileDefinitions.JapaneseTiles;
        for (int cell = 0; cell < japaneseTiles.Length; cell++)
            japanese = japanese.Union(Paint(canvas, (letters + cell % SpaceColonyCaptionDefinitions.JapaneseWidth) * CharacterPixels,
                cell / SpaceColonyCaptionDefinitions.JapaneseWidth * CharacterPixels, japaneseTiles[cell], flight, colours));
        frames.Add(CeresCinematicPictures.JapaneseCaption, Frame(letters * CharacterPixels, 0, SpaceColonyCaptionDefinitions.JapaneseWidth, japaneseRows, japanese));
        ReadOnlySpan<BackgroundTile> planet = intro.CeresDestruction.ZebesMap.Tiles.Span[..(CeresDestructionArtworkFormat.ZebesMapByteCount / sizeof(ushort))];
        ImageColourGroups planetGroups = ImageColourGroups.None;
        for (int cell = 0; cell < planet.Length; cell++)
            planetGroups = planetGroups.Union(Paint(canvas, cell % PlanetCells * CharacterPixels, captionHeight + cell / PlanetCells * CharacterPixels,
                planet[cell], zebes, colours));
        frames.Add(CeresCinematicPictures.ZebesPlanet, Frame(0, captionHeight, PlanetCells, planet.Length / PlanetCells, planetGroups));
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.SixteenColours));
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
        Parts = [],
    };
}
