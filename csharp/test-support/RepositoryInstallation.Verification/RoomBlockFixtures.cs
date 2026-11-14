using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's block compositions and memory image into the block pictures a room plane draws (#1403): each
/// block's four tile words painted from the four-bit characters in the image, mirrored and coloured as each word says, and
/// installed as its part behind and its part in front as each word's priority says, as extraction installs a room's block
/// sheet (#1634).
/// </summary>
internal static class RoomBlockFixtures
{
    /// <summary>Tiles a fixture paints into each block picture: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>
    /// The appearance inputs each of <paramref name="count"/> fixture blocks depends on (#1466): a fixture table names its
    /// pictures by block, so no block depends on any, and every block is shown, which only satisfies the sheet's record of
    /// its blocks.
    /// </summary>
    internal static RoomBlockAppearances Appearances(int count) =>
        new([.. Enumerable.Range(0, count).Select(_ => ImmutableArray<AppearanceInput>.Empty)], null, Enumerable.Range(0, count));

    /// <summary>
    /// The picture of each of <paramref name="definitions"/>' blocks, four tiles each, its characters read from <paramref name="characterWord"/>
    /// in <paramref name="image"/> and coloured from <paramref name="palette"/>, installed as its part behind and its part in
    /// front as extraction installs a room's block sheet (#1634).
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    internal static RoomBlockPictureTable Pictures(ReadOnlySpan<BackgroundTile> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = 0) => Pictured(definitions, image, palette, characterWord).Pictures;

    /// <summary>
    /// The sheet <see cref="Pictures"/> installs <paramref name="definitions"/>' blocks in, and the picture of each block
    /// from it.
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    internal static PicturedBlocks Pictured(ReadOnlySpan<BackgroundTile> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = 0)
    {
        PaintedBlocks painted = Paint(definitions, image, palette, characterWord);
        ComposedSpriteSheet composed = painted.Canvas.ToSplitRoomSheet(painted.Colours, painted.Frames);
        SpriteSheet sheet = SpriteSheetFileConstants.Load(composed.Manifest with { Backdrop = 0 }, "blocks-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices);
        var blocks = new RoomBlockSheet(default(TilesetDefinition), sheet, Appearances(painted.Count));
        return new PicturedBlocks(blocks,
            new RoomBlockPictureTable([.. Enumerable.Range(0, painted.Count).Select(block => (LayeredPicture?)blocks.Picture($"{block}"))],
                new Dictionary<CollectiblePicture, LayeredPicture>()));
    }

    /// <summary>A fixture's block sheet and the picture of each of its blocks.</summary>
    /// <param name="Sheet">The sheet the blocks are installed in.</param>
    /// <param name="Pictures">The picture of each block, from <paramref name="Sheet"/>.</param>
    internal sealed record PicturedBlocks(RoomBlockSheet Sheet, RoomBlockPictureTable Pictures);

    /// <summary>
    /// The whole picture of each of <paramref name="definitions"/>' blocks, painted as <see cref="Pictures"/> paints them but
    /// installed whole, front tiles and back tiles in one picture, as a boss body's pictures are (#1634).
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    internal static SpriteSheetFrame[] WholePictures(ReadOnlySpan<BackgroundTile> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = 0)
    {
        PaintedBlocks painted = Paint(definitions, image, palette, characterWord);
        ComposedSpriteSheet composed = painted.Canvas.ToRoomSheet(painted.Colours, painted.Frames);
        SpriteSheet sheet = SpriteSheetFileConstants.Load(composed.Manifest with { Backdrop = 0 }, "whole-blocks-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices);
        return [.. Enumerable.Range(0, painted.Count).Select(block => sheet.Frame($"{block}"))];
    }

    /// <summary>
    /// Paints each of <paramref name="definitions"/>' blocks, four tiles each, its characters read from
    /// <paramref name="characterWord"/> in <paramref name="image"/> and coloured from <paramref name="palette"/>, with a frame
    /// named by its index.
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    private static PaintedBlocks Paint(ReadOnlySpan<BackgroundTile> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord)
    {
        const int columns = 16, side = RoomBlockSheet.BlockPixels, tile = 8, characterWords = 16;
        if (definitions.IsEmpty || definitions.Length % BlockTiles != 0)
            throw new ArgumentException("Block compositions are whole groups of four tiles.", nameof(definitions));
        int count = definitions.Length / BlockTiles;
        int rows = (count + columns - 1) / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * side, checked((ushort)(rows * side))));
        RgbColor[] colours = palette[..ImageColourGroup.HalfColours].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int block = 0; block < count; block++)
        {
            int left = block % columns * side, top = block / columns * side;
            ImageColourGroups coloured = ImageColourGroups.None;
            for (int corner = 0; corner < 4; corner++)
            {
                BackgroundTile word = definitions[block * BlockTiles + corner];
                for (int part = 0; part < characterWords; part++)
                {
                    int at = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.Tile * characterWords + part);
                    character[part * 2] = image[at];
                    character[part * 2 + 1] = image[at + 1];
                }
                coloured = coloured.Union(canvas.PaintTile(left + corner % 2 * tile, top + corner / 2 * tile, word, character, colours));
            }
            frames.Add($"{block}", new SpriteSheetFrameDocument
            {
                Corner = new Position16<ImagePixels>((ushort)left, (ushort)top),
                Size = new Extent16<ImagePixels>(side, side),
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
            });
        }
        return new PaintedBlocks(canvas, colours, frames, count);
    }

    /// <summary>A fixture's painted blocks: the canvas, the colours they were painted from, a frame for each and their count.</summary>
    /// <param name="Canvas">The painted canvas.</param>
    /// <param name="Colours">The colours the blocks were painted from.</param>
    /// <param name="Frames">Each block's frame, named by its index.</param>
    /// <param name="Count">The blocks painted.</param>
    private sealed record PaintedBlocks(RoomTileCanvas Canvas, RgbColor[] Colours, Dictionary<string, SpriteSheetFrameDocument> Frames, int Count);

    /// <summary>
    /// The library background a fixture loaded into BG2 (#1403): the two tilemap pages at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/>, the first above the second, each word painted from the four-bit characters at
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>, as extraction paints a room's
    /// background image, its pages laid out in one column when <paramref name="pagesInOneColumn"/> is set, otherwise side by side,
    /// as the import lays them out as rooms show them (#1617).
    /// </summary>
    internal static RoomBackgroundImagePlane LoadedBackground(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord,
        int characterWord = 0, bool pagesInOneColumn = false)
    {
        const int tile = 8, characterWords = 16, pageWords = RoomBackgroundImageGeometry.PageTiles * RoomBackgroundImageGeometry.PageTiles;
        Extent16<ImagePixels> size = pagesInOneColumn
            ? new(RoomBackgroundImageGeometryConstants.PagePixels, RoomBackgroundImageGeometry.Pages * RoomBackgroundImageGeometryConstants.PagePixels)
            : new(RoomBackgroundImageGeometry.Pages * RoomBackgroundImageGeometryConstants.PagePixels, RoomBackgroundImageGeometryConstants.PagePixels);
        var canvas = new RoomTileCanvas(size);
        RgbColor[] colours = palette[..ImageColourGroup.HalfColours].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        var words = new ushort[RoomBackgroundImageGeometry.Words];
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int index = 0; index < words.Length; index++)
        {
            int at = SnesVramAddressingConstants.ByteOffsetOfWord(tilemapWord + index);
            words[index] = BinaryPrimitives.ReadUInt16LittleEndian(image.Slice(at, sizeof(ushort)));
            var word = new SnesBgTilemapWord(words[index]);
            for (int part = 0; part < characterWords; part++)
            {
                int from = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.CharacterIndex * characterWords + part);
                character[part * 2] = image[from];
                character[part * 2 + 1] = image[from + 1];
            }
            int page = index / pageWords, within = index % pageWords;
            int across = within % RoomBackgroundImageGeometry.PageTiles * tile, down = within / RoomBackgroundImageGeometry.PageTiles * tile;
            int pageOffset = page * RoomBackgroundImageGeometryConstants.PagePixels;
            coloured = coloured.Union(pagesInOneColumn
                ? canvas.PaintTile(across, pageOffset + down, word, character, colours)
                : canvas.PaintTile(pageOffset + across, down, word, character, colours));
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = size,
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
            },
        };
        // The image is installed as its part behind and its part in front, as extraction installs a room's background (#1634).
        ComposedSpriteSheet composed = canvas.ToSplitRoomSheet(colours, frames);
        SpriteSheet sheet = SpriteSheetFileConstants.Load(composed.Manifest, "loaded-background-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices);
        var background = new RoomBackgroundImage("loaded-background-fixture", sheet, canvas.Size, []);
        return new RoomBackgroundImagePlane(background, new LayeredPicture?[background.CellCount],
            RoomBackgroundImagePlacement.Whole(background));
    }
}
