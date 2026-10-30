using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into frontend tile layers (#1403): the characters at a character base become a
/// pre-coloured tile set, composed under the fixture's palette as extraction composes installed artwork, and a tilemap
/// becomes the cells a producer captures from it.
/// </summary>
internal static class VramTileFixtures
{
    /// <summary>
    /// The tile set of all <see cref="ScreenLayerPictures.CharacterIndices"/> <paramref name="depth"/> characters from
    /// <paramref name="characterWord"/> in <paramref name="image"/>, read as VRAM addresses wrap, coloured from <paramref name="palette"/>.
    /// </summary>
    internal static ScreenLayerPictures Tiles(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord, TileInkDepth depth) =>
        Tiles(image, palette, characterWord, depth, [.. Enumerable.Range(0, ScreenLayerPictures.CharacterIndices)]);

    /// <summary>
    /// The tile set of <paramref name="characters"/>, <paramref name="depth"/> characters by their index from
    /// <paramref name="characterWord"/> in <paramref name="image"/>, read as VRAM addresses wrap and coloured from
    /// <paramref name="palette"/>; the set shows nothing for the other indices.
    /// </summary>
    internal static ScreenLayerPictures Tiles(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int characterWord, TileInkDepth depth,
        IReadOnlyCollection<int> characters)
    {
        int[] shown = [.. characters.Distinct().Order()];
        int words = ScreenTileSheet.CharacterWordsOf(depth);
        var bytes = new byte[shown.Length * words * sizeof(ushort)];
        for (int place = 0; place < shown.Length; place++)
        {
            for (int word = 0; word < words; word++)
            {
                int offset = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + Ensure.BetweenInclusive(0, ScreenLayerPictures.CharacterIndices - 1, shown[place]) * words + word);
                bytes[(place * words + word) * sizeof(ushort)] = image[offset];
                bytes[(place * words + word) * sizeof(ushort) + 1] = image[offset + 1];
            }
        }
        ComposedSpriteSheet composed = ScreenTileSheetComposerConstants.Compose(bytes, depth, palette);
        SpriteSheet sheet = SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false),
            $"vram-fixture-{characterWord:X4}-{depth}", composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
        var tiles = new ScreenTileSheet($"vram-fixture-{characterWord:X4}-{depth}", sheet, depth);
        var pictures = new ScreenTilePicture?[ScreenLayerPictures.CharacterIndices];
        for (int place = 0; place < shown.Length; place++)
            pictures[shown[place]] = new ScreenTilePicture(tiles, place);
        return new ScreenLayerPictures([.. pictures], depth);
    }

    /// <summary>
    /// The cells of the <paramref name="size"/> tilemap at <paramref name="tilemapWord"/> in <paramref name="image"/>, showing
    /// the tile set of the <paramref name="depth"/> characters from <paramref name="characterWord"/> coloured from <paramref name="palette"/>.
    /// </summary>
    internal static ScreenTileMap Cells(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord, int characterWord, TileInkDepth depth,
        Extent16<TilemapCells> size) =>
        ScreenTileMapConstants.Capture(image, tilemapWord, size, Tiles(image, palette, characterWord, depth));
}
