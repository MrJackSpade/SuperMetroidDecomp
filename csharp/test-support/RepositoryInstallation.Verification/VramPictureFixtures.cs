using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Turns a fixture's memory image into pictured backgrounds (#1467): a tilemap's cells over the characters at a character
/// base become one picture of the whole map on a pictured plane, or a grid of one-cell pictures each mirrored as its cell
/// mirrors it. The import's frontend picture composer paints them under the fixture's palette, naming the gameplay
/// background set of each colour group as installed room sheets do, so gameplay and frontend frames both colour them (#1493).
/// </summary>
internal static class VramPictureFixtures
{
    /// <summary>The frame of a plane's whole-map picture.</summary>
    private const string MapFrame = "map";

    /// <summary>The size of a grid cell's picture.</summary>
    private static readonly Extent16<TilemapCells> CellSize = new(1, 1);

    /// <summary>
    /// The cells of the <paramref name="size"/> tilemap at <paramref name="tilemapWord"/> in <paramref name="image"/>, row by
    /// row from the top-left cell, read as a background reads a map of whole screens, VRAM addresses wrapping.
    /// </summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    internal static BackgroundTile[] Tilemap(ReadOnlySpan<byte> image, int tilemapWord, Extent16<TilemapCells> size)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        var geometry = new BgTilemapGeometry(size);
        var cells = new BackgroundTile[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                int offset = SnesVramAddressingConstants.ByteOffsetOfWord(geometry.EntryWordAddress(VramWordRange.Require(tilemapWord),
                    new Position16<TilemapCells>((ushort)x, (ushort)y)));
                var word = new SnesBgTilemapWord(LittleEndianBytes.Word(image[offset], image[offset + 1]));
                cells[y * size.X + x] = new BackgroundTile(word.CharacterIndex, word.PaletteIndex, word.FlipHorizontally, word.FlipVertically, word.HasPriority);
            }
        }
        return cells;
    }

    /// <summary>
    /// Every character a map word can name, <paramref name="depth"/> characters from <paramref name="characterWord"/> in
    /// <paramref name="image"/>, read as VRAM addresses wrap.
    /// </summary>
    internal static ScreenCharacters Characters(ReadOnlySpan<byte> image, int characterWord, TileInkDepth depth)
    {
        var characters = new ScreenCharacters(depth);
        var bytes = new byte[SnesBgTilemapWord.CharacterIndices * characters.CharacterBytes];
        for (int word = 0; word < bytes.Length / sizeof(ushort); word++)
        {
            int offset = SnesVramAddressingConstants.ByteOffsetOfWord(characterWord + word);
            bytes[word * sizeof(ushort)] = image[offset];
            bytes[word * sizeof(ushort) + 1] = image[offset + 1];
        }
        return characters.Place(0, bytes);
    }

    /// <summary>
    /// A <paramref name="size"/> plane showing one picture of the tilemap at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/> over its <paramref name="depth"/> characters from <paramref name="characterWord"/>, coloured
    /// from <paramref name="palette"/>: each cell's tile in its colour variant, mirroring and priority tier.
    /// </summary>
    internal static PicturedPlane Plane(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord, int characterWord, TileInkDepth depth,
        Extent16<TilemapCells> size)
    {
        BackgroundTile[] cells = Tilemap(image, tilemapWord, size);
        SpriteSheet sheet = Load(ScreenPictureSheetComposerConstants.ComposeRoom([ScreenPictureSubject.Page(MapFrame, cells, size.X)],
            Characters(image, characterWord, depth), palette), $"vram-plane-{tilemapWord:X4}-{characterWord:X4}-{depth}");
        return new PicturedPlane(size, [new PlacedPlanePicture(sheet.Frame(MapFrame), Position16<TilePlanePixels>.Zero)]);
    }

    /// <summary>
    /// A <paramref name="size"/> grid whose cells show the tilemap at <paramref name="tilemapWord"/> in
    /// <paramref name="image"/> over its <paramref name="depth"/> characters from <paramref name="characterWord"/>, coloured
    /// from <paramref name="palette"/>: each cell the unmirrored picture of its tile in its colour variant and priority tier,
    /// which the grid mirrors as the cell does.
    /// </summary>
    internal static PictureGrid Grid(ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette, int tilemapWord, int characterWord, TileInkDepth depth,
        Extent16<TilemapCells> size)
    {
        BackgroundTile[] cells = Tilemap(image, tilemapWord, size);
        BackgroundTile[] unmirrored = [.. cells.Select(Unmirrored).Distinct()];
        SpriteSheet sheet = Load(ScreenPictureSheetComposerConstants.ComposeRoom(
            [.. unmirrored.Select(tile => ScreenPictureSubject.Block(CellFrame(tile), default, CellSize, [tile]))],
            Characters(image, characterWord, depth), palette), $"vram-grid-{tilemapWord:X4}-{characterWord:X4}-{depth}");
        return new PictureGrid(size, [.. cells.Select(cell => (GridPicture?)new GridPicture(sheet.Frame(CellFrame(Unmirrored(cell))), cell.Flip))]);
    }

    /// <summary><paramref name="tile"/> without its mirroring.</summary>
    private static BackgroundTile Unmirrored(BackgroundTile tile) => tile with { FlipHorizontally = false, FlipVertically = false };

    /// <summary>The frame of an unmirrored tile's one-cell picture.</summary>
    private static string CellFrame(BackgroundTile tile) => $"cell-{tile.Tile}-{tile.ColourVariant}-{tile.HighPriority}";

    /// <summary>The composed sheet as an installed sheet would load.</summary>
    private static SpriteSheet Load(ComposedSpriteSheet composed, string name) =>
        SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false), name,
            composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartGroups);
}
