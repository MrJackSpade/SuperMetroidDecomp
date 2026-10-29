using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392): the full picture of a 32-tile-wide tilemap a room shows on BG2,
/// coloured with its tileset's palette over the characters the room loads, and the tiles it pictures. A library
/// background pictures the two BG2 pages it loads, the first above the second; the scrolling sky pictures all its rows.
/// </summary>
public sealed class RoomBackgroundImage
{
    /// <summary>Tiles along a tilemap page's side.</summary>
    public const int PageTiles = 32;

    /// <summary>The tilemap pages the image pictures.</summary>
    public const int Pages = 2;

    /// <summary>Tiles in a library background's two pages.</summary>
    public const int Words = Pages * PageTiles * PageTiles;

    /// <summary>Pixels along a tilemap page's side.</summary>
    public const int PagePixels = PageTiles * 8;

    /// <summary>The frame of an image's sheet that pictures its whole tilemap.</summary>
    public const string FrameName = "pages";

    /// <summary>Tiles in one 16x16 cell of an image.</summary>
    public const int CellTiles = 4;

    /// <summary>Checks the tiles fill whole pairs of 32-tile rows and the sheet's picture covers them.</summary>
    /// <exception cref="InvalidDataException">The tiles do not fill pairs of rows or the picture is not their size.</exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, ImmutableArray<BackgroundTile> tiles)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteSheetFrame picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (tiles.IsDefaultOrEmpty || tiles.Length % (2 * PageTiles) != 0)
            throw new InvalidDataException($"Room background {name} pictures {tiles.Length} tiles, not whole pairs of {PageTiles}-tile rows.");
        if (picture.Size.X != PagePixels || picture.Size.Y != tiles.Length / PageTiles * 8)
            throw new InvalidDataException($"Room background {name} is {picture.Size.X}x{picture.Size.Y}, not its {tiles.Length / PageTiles} rows.");
        if (picture.Sheet.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room background {name} is not coloured from the background palette.");
        Name = name;
        Sheet = sheet;
        Picture = picture;
        Tiles = tiles;
    }

    /// <summary>The background's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of both pages, the first above the second.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The tiles pictured, row by row, 32 to a row.</summary>
    public ImmutableArray<BackgroundTile> Tiles { get; }

    /// <summary>The image's 16x16 cells: two rows by two columns each.</summary>
    public int CellCount => Tiles.Length / CellTiles;

    /// <summary>The installed name of the sky <paramref name="variant"/>'s rows in a room of <paramref name="tileset"/>.</summary>
    public static string SkyNameOf(ScrollingSkyChunkTable variant, TilesetDefinition tileset) => $"Sky{variant}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>Checks <paramref name="tiles"/> are the tiles the image pictures, as a sky's rows must be.</summary>
    /// <exception cref="InvalidDataException">The tiles differ.</exception>
    public void AssertPictures(ReadOnlySpan<BackgroundTile> tiles)
    {
        if (!tiles.SequenceEqual(Tiles.AsSpan()))
            throw new InvalidDataException($"The tilemap shown is not the one room background {Name} pictures.");
    }

    /// <summary>
    /// The installed name of <paramref name="background"/>'s picture in a room of <paramref name="tileset"/>: the picture a
    /// room entered through <paramref name="door"/> shows when that door shows its own picture, otherwise, for no door,
    /// the ordinary one.
    /// </summary>
    public static string NameOf(LibraryBackground background, DoorDefinition? door, TilesetDefinition tileset) =>
        $"{Ensure.NotNull(background).Name}{(door is null ? "" : "." + door.Name)}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>
    /// The 16x16 cells of 32-tile-wide <paramref name="tiles"/>, row by row, each its four tiles: top left, top right,
    /// bottom left, bottom right.
    /// </summary>
    public static IEnumerable<BackgroundTile[]> Cells(IReadOnlyList<BackgroundTile> tiles)
    {
        Ensure.NotNull(tiles);
        for (int cell = 0; cell < tiles.Count / CellTiles; cell++)
        {
            int first = CellWord(cell);
            yield return [tiles[first], tiles[first + 1], tiles[first + PageTiles], tiles[first + PageTiles + 1]];
        }
    }

    /// <summary>
    /// The name of the picture the block sheet holds of a cell whose tiles are <paramref name="tiles"/> over
    /// <paramref name="shown"/> (#1392): a cell whose characters the room rebinds after loading draws it over the image.
    /// </summary>
    /// <param name="tiles">The cell's four tiles: top left, top right, bottom left, bottom right.</param>
    /// <param name="shown">What shows over each tile's character, in the same order.</param>
    /// <exception cref="ArgumentException">Either span does not hold four entries.</exception>
    public static string CellPictureName(ReadOnlySpan<BackgroundTile> tiles, ReadOnlySpan<BgArtworkShown> shown)
    {
        Ensure.LengthEqual(tiles, CellTiles);
        Ensure.LengthEqual(shown, CellTiles);
        var name = new StringBuilder();
        for (int tile = 0; tile < CellTiles; tile++)
            name.Append(tile == 0 ? "" : ".").Append(TileName(tiles[tile]));
        for (int tile = 0; tile < CellTiles; tile++)
            name.Append('|').Append(shown[tile].Artwork.Name).Append('+')
                .Append(shown[tile].WordOffset.ToString("X4", CultureInfo.InvariantCulture));
        return name.ToString();
    }

    /// <summary>
    /// The part of a cell picture's name that names one tile: its character, colour variant, and an <c>h</c>, <c>v</c> and
    /// <c>p</c> for each of its mirrorings and its upper priority tier.
    /// </summary>
    private static string TileName(BackgroundTile tile) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{tile.Tile:X3}c{tile.ColourVariant}{(tile.FlipHorizontally ? "h" : "")}{(tile.FlipVertically ? "v" : "")}{(tile.HighPriority ? "p" : "")}");

    /// <summary>The first tile of cell <paramref name="cell"/>: sixteen cells to a pair of 32-tile rows.</summary>
    public static int CellWord(int cell) => cell / (PageTiles / 2) * 2 * PageTiles + cell % (PageTiles / 2) * 2;
}
