using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392, #1466): the full picture of a 32-tile-wide tilemap a room shows on
/// BG2, coloured with its tileset's palette over what the room loads, and the cells whose pictures change with the room's
/// appearance inputs. A library background pictures the two BG2 pages it loads, the first above the second; the scrolling
/// sky pictures all its rows.
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

    /// <summary>Pixels along a cell's side.</summary>
    private const int CellPixels = 16;

    /// <summary>Checks the picture spans a page's width and whole cell rows, and that each changing cell lies in it once.</summary>
    /// <exception cref="InvalidDataException">The picture is not whole cell rows across a page, or a cell lies outside it or repeats.</exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, IEnumerable<RoomBackgroundCell> cells)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteSheetFrame picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (picture.Size.X != PagePixels || picture.Size.Y == 0 || picture.Size.Y % CellPixels != 0)
            throw new InvalidDataException($"Room background {name} is {picture.Size.X}x{picture.Size.Y}, not whole cell rows across a page.");
        if (picture.Sheet.Colours != SpriteImageColours.Background)
            throw new InvalidDataException($"Room background {name} is not coloured from the background palette.");
        Name = name;
        Sheet = sheet;
        Picture = picture;
        CellCount = PagePixels / CellPixels * (picture.Size.Y / CellPixels);
        Cells = [.. Ensure.NotNull(cells)];
        if (Cells.Any(cell => cell.Cell < 0 || cell.Cell >= CellCount) || Cells.DistinctBy(cell => cell.Cell).Count() != Cells.Count)
            throw new InvalidDataException($"Room background {name} records a changing cell outside its {CellCount} cells, or one twice.");
    }

    /// <summary>The background's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of both pages, the first above the second.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The image's 16x16 cells: two rows by two columns of tiles each.</summary>
    public int CellCount { get; }

    /// <summary>The cells whose pictures change with the room's appearance inputs; every other cell always shows the image.</summary>
    public IReadOnlyList<RoomBackgroundCell> Cells { get; }

    /// <summary>The installed name of the sky <paramref name="variant"/>'s rows in a room of <paramref name="tileset"/>.</summary>
    public static string SkyNameOf(ScrollingSkyChunkTable variant, TilesetDefinition tileset) => $"Sky{variant}.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>
    /// The installed name of <paramref name="background"/>'s picture in a room of <paramref name="tileset"/>: the picture a
    /// room entered through <paramref name="door"/> shows when that door shows its own picture, otherwise, for no door,
    /// the ordinary one.
    /// </summary>
    public static string NameOf(LibraryBackground background, DoorDefinition? door, TilesetDefinition tileset) =>
        $"{Ensure.NotNull(background).Name}{(door is null ? "" : "." + door.Name)}.{RoomBlockSheet.NameOf(tileset)}";
}
