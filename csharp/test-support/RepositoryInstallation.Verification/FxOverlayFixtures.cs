using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's tile words into what the effect overlay shows (#1403, #1514, #1706): the words laid row by row into
/// pictures, each painted, mirrored and coloured as the word says from its two-bit character and installed as its part
/// behind and its part in front as each word's priority says, as extraction installs a room's effect overlay sheet (#1634).
/// An effect's artwork is cut from the rows the original game wrote the effect's transfer to, as the import cuts it
/// (<see cref="RoomEffectTransferCuts"/>); a fixture's rows of a whole plane are shown through <see cref="Rows"/>.
/// </summary>
internal static class FxOverlayFixtures
{
    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>
    /// The picture of the gameplay overlay tilemap's whole plane in <paramref name="image"/>, its words' characters read from
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    internal static LayeredPicture PlaneFromVram(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord) =>
        Picture(PlaneWords(image), image, palette, characterWord);

    /// <summary>
    /// The artwork effect <paramref name="type"/> shows of the gameplay overlay tilemap in <paramref name="image"/>: its rows
    /// from the row the original game wrote an effect's transfer to, painted as <see cref="PlaneFromVram"/> paints them.
    /// </summary>
    internal static FxOverlayArtwork ArtworkFromVram(RoomFxType type, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord, LiquidTransferRows? liquidCut = null) =>
        ArtworkFromPlaneWords(type, PlaneWords(image), image, palette, characterWord, liquidCut);

    /// <summary>
    /// The artwork effect <paramref name="type"/> shows of a whole plane of <paramref name="plane"/> tile words: its rows from
    /// the row the original game wrote an effect's transfer to, cut as the import cuts them or, for a liquid whose rows
    /// repeat nowhere such as a plane of seeded words, at <paramref name="liquidCut"/>, their characters read from
    /// <paramref name="characterWord"/> in <paramref name="image"/>.
    /// </summary>
    internal static FxOverlayArtwork ArtworkFromPlaneWords(RoomFxType type, ReadOnlySpan<ushort> plane, ReadOnlySpan<byte> image,
        ReadOnlySpan<RgbColor> palette, int characterWord = GameplayVramLayout.HudCharacterWord, LiquidTransferRows? liquidCut = null)
    {
        Ensure.LengthEqual(plane, FxOverlayPlaneConstants.Columns * FxOverlayPlaneConstants.Rows);
        ReadOnlySpan<ushort> transfer = plane[(FxOverlayPlaneConstants.EffectPageFirstRow * FxOverlayPlaneConstants.Columns)..];
        int columns = FxOverlayPlaneConstants.Columns;
        if (!RoomFxTypes.ShowsLiquid(type))
        {
            int first = RoomEffectTransferCuts.AtmosphereFirstRow(transfer);
            return new FxOverlayArtwork(new AtmosphereArtwork(Picture(transfer[(first * columns)..], image, palette, characterWord)));
        }
        // The strip and the body share one sheet, as an installed overlay sheet holds both.
        LiquidTransferRows cut = liquidCut ?? RoomEffectTransferCuts.Liquid(transfer);
        SpriteSheet sheet = Sheet(transfer[..((cut.BodyFirstRow + cut.BodyRows) * columns)], image, palette, characterWord,
            [("surface", 0, cut.BodyFirstRow, new Offset16<ImagePixels>(0, RoomEffectTransferCuts.SurfaceRow)),
             ("body", cut.BodyFirstRow, cut.BodyRows, default)]);
        return new FxOverlayArtwork(new LiquidArtwork(PictureParts.Of(sheet, "surface"), PictureParts.Of(sheet, "body")));
    }

    /// <summary>
    /// The artwork <paramref name="plane"/> shows with the installed <paramref name="pages"/>, painted from the characters at
    /// <paramref name="characterWord"/> in <paramref name="image"/>, or nothing where it shows no effect.
    /// </summary>
    internal static FxOverlayArtwork ArtworkOf(FxOverlayPlane plane, RoomFxLayer3Pages pages, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord) =>
        Ensure.NotNull(plane).EffectPage is not { } type ? new FxOverlayArtwork(null)
            : ArtworkFromPlaneWords(type, plane.WordsIn(pages), image, palette, characterWord);

    /// <summary>
    /// The atmosphere a whole plane's picture shows of its <paramref name="rows"/> tile rows from row
    /// <paramref name="firstRow"/>, repeating down and the plane's width repeating across, as the original game's plane
    /// showed a window of its rows: the plane's parts cut to those rows.
    /// </summary>
    internal static AtmosphereArtwork Rows(LayeredPicture plane, int firstRow, int rows)
    {
        Ensure.NotNull(plane);
        Extent16<ImagePixels> pictured = plane.Size ?? throw new InvalidOperationException("An overlay fixture's plane installs neither part.");
        var offset = new Offset16<ImagePixels>(0, firstRow * TilePixels);
        var size = new Extent16<ImagePixels>(pictured.X, checked((ushort)(rows * TilePixels)));
        if (offset.Y + size.Y > pictured.Y)
            throw new ArgumentOutOfRangeException(nameof(rows), $"Rows {firstRow} through {firstRow + rows - 1} leave the {pictured.Y / TilePixels}-row plane.");
        SpriteSheetFrame? Cut(SpriteSheetFrame? part) => part is null ? null
            : new SpriteSheetFrame(part.Sheet, part.Corner + offset, size, default) { Depth = part.Depth, Blends = part.Blends };
        return new AtmosphereArtwork(new LayeredPicture(Cut(plane.Behind), Cut(plane.InFront)));
    }

    /// <summary>
    /// The picture <paramref name="words"/> show, laid row by row a plane's width to a row, their characters read from
    /// <paramref name="characterWord"/> in <paramref name="image"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    internal static LayeredPicture Picture(ReadOnlySpan<ushort> words, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = GameplayVramLayout.HudCharacterWord) =>
        PictureParts.Of(Sheet(words, image, palette, characterWord, [("overlay", 0, words.Length / FxOverlayPlaneConstants.Columns, default)]), "overlay");

    /// <summary>
    /// One sheet of what <paramref name="words"/> show, laid as <see cref="Picture"/> lays them, with each of
    /// <paramref name="frames"/> its named run of tile rows and its origin.
    /// </summary>
    private static SpriteSheet Sheet(ReadOnlySpan<ushort> words, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord,
        (string Name, int FirstRow, int Rows, Offset16<ImagePixels> Origin)[] frames)
    {
        const int characterWords = 8;
        int columns = FxOverlayPlaneConstants.Columns;
        if (words.IsEmpty || words.Length % columns != 0)
            throw new ArgumentException($"An overlay fixture lays whole rows of {columns} words.", nameof(words));
        int rows = words.Length / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * TilePixels, checked((ushort)(rows * TilePixels))));
        RgbColor[] colours = palette[..ImageColourGroup.HalfColours].ToArray();
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
        var documents = frames.ToDictionary(frame => frame.Name,
            frame => WrittenPictureSheetComposer.Frame(canvas, frame.FirstRow, frame.Rows, WrittenPictureLayer.FxOverlayAndHud) with { Origin = frame.Origin },
            StringComparer.Ordinal);
        ComposedSpriteSheet composed = canvas.ToSplitRoomSheet(colours, documents, SpriteImageColours.TwoBppBackground);
        return SpriteSheetFileConstants.Load(composed.Manifest, "overlay-fixture", composed.Size, composed.Pixels, composed.PaletteIndices);
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
