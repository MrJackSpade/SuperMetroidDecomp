using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's tile words into what the effect overlay shows (#1403, #1514): the words laid row by row into one
/// picture, each painted, mirrored, coloured and prioritised as the word says from its two-bit character, as extraction
/// paints a room's effect overlay sheet. A fixture's page is the rows the original game wrote the effect's page to; a
/// fixture's whole plane is placed through <see cref="Rows"/>.
/// </summary>
internal static class FxOverlayFixtures
{
    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>
    /// The picture of the gameplay overlay tilemap's whole plane in <paramref name="image"/>, its words' characters read from
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    internal static SpriteSheetFrame PlaneFromVram(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord) =>
        Picture(PlaneWords(image), image, palette, characterWord);

    /// <summary>
    /// The page the gameplay overlay tilemap in <paramref name="image"/> shows: its rows from the row the original game
    /// wrote an effect's page to, painted as <see cref="PlaneFromVram"/> paints them.
    /// </summary>
    internal static FxOverlayPage PageFromVram(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord) =>
        PageFromPlaneWords(PlaneWords(image), image, palette, characterWord);

    /// <summary>
    /// The page a whole plane of <paramref name="plane"/> tile words shows: its rows from the row the original game wrote an
    /// effect's page to, their characters read from <paramref name="characterWord"/> in <paramref name="image"/>.
    /// </summary>
    internal static FxOverlayPage PageFromPlaneWords(ReadOnlySpan<ushort> plane, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord)
    {
        Ensure.LengthEqual(plane, FxOverlayPlaneConstants.Columns * FxOverlayPlaneConstants.Rows);
        return new FxOverlayPage(Picture(plane[(FxOverlayPlaneConstants.EffectPageFirstRow * FxOverlayPlaneConstants.Columns)..], image, palette, characterWord));
    }

    /// <summary>
    /// The page <paramref name="plane"/> shows with the installed <paramref name="pages"/>, painted from the characters at
    /// <paramref name="characterWord"/> in <paramref name="image"/>, or nothing where it shows no page.
    /// </summary>
    internal static FxOverlayPage PageOf(FxOverlayPlane plane, RoomFxLayer3Pages pages, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord) =>
        Ensure.NotNull(plane).EffectPage is null ? new FxOverlayPage(null) : PageFromPlaneWords(plane.WordsIn(pages), image, palette, characterWord);

    /// <summary>
    /// The overlay a whole plane's picture shows from tile row <paramref name="firstRow"/>, <paramref name="rows"/> rows
    /// repeating down and the plane's width repeating across, as the original game's plane showed a window of its rows.
    /// </summary>
    internal static OverlayPicture Rows(SpriteSheetFrame plane, int firstRow, int rows)
    {
        Ensure.NotNull(plane);
        var size = new Extent16<OverlayPixels>(plane.Size.X, rows * TilePixels);
        return new OverlayPicture(plane, new Position16<ImagePixels>(0, firstRow * TilePixels), size, Position16<OverlayPixels>.Zero, size);
    }

    /// <summary>
    /// The picture <paramref name="words"/> show, laid row by row a plane's width to a row, their characters read from
    /// <paramref name="characterWord"/> in <paramref name="image"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    internal static SpriteSheetFrame Picture(ReadOnlySpan<ushort> words, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord)
    {
        const int characterWords = 8;
        int columns = FxOverlayPlaneConstants.Columns;
        if (words.IsEmpty || words.Length % columns != 0)
            throw new ArgumentException($"An overlay fixture lays whole rows of {columns} words.", nameof(words));
        int rows = words.Length / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * TilePixels, checked((ushort)(rows * TilePixels))));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        for (int cell = 0; cell < words.Length; cell++)
        {
            var word = new SnesBgTilemapWord(words[cell]);
            for (int part = 0; part < characterWords; part++)
            {
                int at = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.CharacterIndex * characterWords + part);
                character[part * 2] = image[at];
                character[part * 2 + 1] = image[at + 1];
            }
            canvas.PaintTwoBppTile(cell % columns * TilePixels, cell / columns * TilePixels, word, character, colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            ["overlay"] = WrittenPictureSheetComposer.Frame(canvas, 0, rows, WrittenPictureLayer.FxOverlayAndHud),
        };
        ComposedSpriteSheet composed = canvas.ToRoomSheet(colours, frames, SpriteImageColours.TwoBppBackground);
        SpriteSheet sheet = SpriteSheetFileConstants.Load(new MemoryStream(SpriteSheetManifestWriter.Write(composed.Manifest), writable: false), "overlay-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities);
        return sheet.Frame("overlay");
    }

    /// <summary>The gameplay overlay tilemap's words in <paramref name="image"/>, row by row.</summary>
    private static ushort[] PlaneWords(ReadOnlySpan<byte> image)
    {
        var words = new ushort[FxOverlayPlaneConstants.Columns * FxOverlayPlaneConstants.Rows];
        for (int cell = 0; cell < words.Length; cell++)
        {
            int at = SnesVramAddressingConstants.ByteOffsetOfWord(GameplayVramLayoutConstants.HudTilemapWord + cell);
            words[cell] = LittleEndianBytes.Word(image[at], image[at + 1]);
        }
        return words;
    }
}
