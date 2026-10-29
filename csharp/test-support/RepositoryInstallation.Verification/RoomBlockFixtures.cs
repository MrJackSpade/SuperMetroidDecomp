using System.Buffers.Binary;
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
/// block's four tile words painted from the four-bit characters in the image, mirrored, coloured and prioritised as
/// each word says, as extraction paints a room's block sheet.
/// </summary>
internal static class RoomBlockFixtures
{
    /// <summary>Tiles a fixture paints into each block picture: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>
    /// The appearance inputs each of <paramref name="count"/> fixture blocks depends on (#1466): a fixture table names its
    /// pictures by block, so no block depends on any, which only satisfies the sheet's record of its blocks.
    /// </summary>
    internal static RoomBlockAppearances Appearances(int count) =>
        new(Enumerable.Range(0, count).Select(_ => Array.Empty<AppearanceInput>()), null);

    /// <summary>
    /// The picture of each of <paramref name="definitions"/>' blocks, four tiles each, its characters read from <paramref name="characterWord"/>
    /// in <paramref name="image"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    internal static RoomBlockPictureTable Pictures(ReadOnlySpan<BackgroundTile> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette,
        int characterWord = 0)
    {
        const int columns = 16, side = RoomBlockSheet.BlockPixels, tile = 8, characterWords = 16;
        if (definitions.IsEmpty || definitions.Length % BlockTiles != 0)
            throw new ArgumentException("Block compositions are whole groups of four tiles.", nameof(definitions));
        int count = definitions.Length / BlockTiles;
        int rows = (count + columns - 1) / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * side, checked((ushort)(rows * side))));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
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
                Parts = [],
            });
        }
        ComposedSpriteSheet composed = canvas.ToRoomSheet(colours, frames);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "blocks-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
        return new RoomBlockPictureTable(new RoomBlockSheet(default(TilesetDefinition), sheet, Appearances(count)),
            [.. Enumerable.Range(0, count).Select(block => (SpriteSheetFrame?)sheet.Frame($"{block}"))]);
    }

    /// <summary>
    /// The library background a fixture loaded into BG2 (#1403): the two tilemap pages at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/>, the first above the second, each word painted from the four-bit characters at
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>, as extraction paints a room's
    /// background image.
    /// </summary>
    internal static RoomBackgroundImagePlane LoadedBackground(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord,
        int characterWord = 0)
    {
        const int tile = 8, characterWords = 16, pageWords = RoomBackgroundImage.PageTiles * RoomBackgroundImage.PageTiles;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        var words = new ushort[RoomBackgroundImage.Words];
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
            coloured = coloured.Union(canvas.PaintTile(within % RoomBackgroundImage.PageTiles * tile,
                page * RoomBackgroundImage.PagePixels + within / RoomBackgroundImage.PageTiles * tile, word, character, colours));
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels),
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
                Parts = [],
            },
        };
        ComposedSpriteSheet composed = canvas.ToRoomSheet(colours, frames);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "loaded-background-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
        var background = new RoomBackgroundImage("loaded-background-fixture", sheet, []);
        return new RoomBackgroundImagePlane(background, new SpriteSheetFrame?[background.CellCount]);
    }
}
