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
/// Turns a fixture's block definitions and memory image into the block pictures a room plane draws (#1403): each
/// definition's four tile words painted from the four-bit characters in the image, mirrored, coloured and prioritised as
/// each word says, as extraction paints a room's block sheet.
/// </summary>
internal static class RoomBlockFixtures
{
    /// <summary>
    /// The picture of each of <paramref name="definitions"/>' blocks, its characters read from <paramref name="characterWord"/>
    /// in <paramref name="image"/> and coloured from <paramref name="palette"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The definitions are not whole blocks.</exception>
    [PpuLayoutBoundary("exact hardware test: decodes a fixture's block definitions and four-bit characters into block pictures")]
    internal static RoomBlockPictureTable Pictures(ReadOnlySpan<byte> definitions, ReadOnlySpan<byte> image, ReadOnlySpan<Bgr555> palette,
        int characterWord = 0)
    {
        const int columns = 16, side = RoomBlockSheet.BlockPixels, tile = 8, characterWords = 16;
        if (definitions.IsEmpty || definitions.Length % RoomLevelPlane.DefinitionBytes != 0)
            throw new ArgumentException("Block definitions are whole groups of four tilemap words.", nameof(definitions));
        int count = definitions.Length / RoomLevelPlane.DefinitionBytes;
        int rows = (count + columns - 1) / columns;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(columns * side, checked((ushort)(rows * side))));
        Bgr555[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        for (int block = 0; block < count; block++)
        {
            int left = block % columns * side, top = block / columns * side;
            SpritePaletteRows coloured = SpritePaletteRows.None;
            for (int corner = 0; corner < 4; corner++)
            {
                var word = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(
                    definitions.Slice(block * RoomLevelPlane.DefinitionBytes + corner * sizeof(ushort))));
                for (int part = 0; part < characterWords; part++)
                {
                    int at = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word.CharacterIndex * characterWords + part);
                    character[part * 2] = image[at];
                    character[part * 2 + 1] = image[at + 1];
                }
                coloured |= canvas.PaintTile(left + corner % 2 * tile, top + corner / 2 * tile, word, character, colours);
            }
            frames.Add($"{block}", new SpriteSheetFrameDocument
            {
                Corner = new Position16<ImagePixels>((ushort)left, (ushort)top),
                Size = new Extent16<ImagePixels>(side, side),
                Origin = default,
                Rows = coloured,
                Parts = [],
            });
        }
        ComposedSpriteSheet composed = canvas.ToSheet(colours, frames);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "blocks-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
        return new RoomBlockPictureTable(new RoomBlockSheet(default(TilesetDefinition), sheet),
            [.. Enumerable.Range(0, count).Select(block => (SpriteSheetFrame?)sheet.Frame($"{block}"))]);
    }

    /// <summary>
    /// The library background a fixture loaded into BG2 (#1403): the two tilemap pages at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/>, the first above the second, each word painted from the four-bit characters at
    /// <paramref name="characterWord"/> and coloured from <paramref name="palette"/>, as extraction paints a room's
    /// background image.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: decodes a fixture's BG2 tilemap and four-bit characters into a background image")]
    internal static RoomBackgroundImagePlane LoadedBackground(ReadOnlySpan<byte> image, ReadOnlySpan<Bgr555> palette, int tilemapWord,
        int characterWord = 0)
    {
        const int tile = 8, characterWords = 16, pageWords = RoomBackgroundImage.PageTiles * RoomBackgroundImage.PageTiles;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels));
        Bgr555[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        var character = new byte[characterWords * sizeof(ushort)];
        var words = new ushort[RoomBackgroundImage.Words];
        SpritePaletteRows coloured = SpritePaletteRows.None;
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
            coloured |= canvas.PaintTile(within % RoomBackgroundImage.PageTiles * tile,
                page * RoomBackgroundImage.PagePixels + within / RoomBackgroundImage.PageTiles * tile, word, character, colours);
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [RoomBackgroundImage.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>(RoomBackgroundImage.PagePixels, RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels),
                Origin = default,
                Rows = coloured,
                Parts = [],
            },
        };
        ComposedSpriteSheet composed = canvas.ToSheet(colours, frames);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), "loaded-background-fixture",
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
        var background = new RoomBackgroundImage("loaded-background-fixture", sheet, [.. words]);
        return new RoomBackgroundImagePlane(background, new SpriteSheetFrame?[background.CellCount]);
    }
}
