using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392, #1466, #1555): the picture a room shows on the background, coloured
/// with its own colours, and the cells whose pictures change with the room's appearance inputs. A library background
/// pictures its two halves, the first above the second, which a room places on the background
/// (<see cref="RoomBackgroundImagePlacement"/>); the scrolling sky pictures all its rows.
/// </summary>
public sealed class RoomBackgroundImage
{
    /// <summary>The frame of an image's sheet that pictures the whole background.</summary>
    public const string FrameName = "pages";

    /// <summary>Pixels along a cell's side.</summary>
    public const int CellPixels = 16;

    /// <summary>Checks the picture is whole cells across and down, and that each changing cell lies in it once.</summary>
    /// <exception cref="InvalidDataException">
    /// The picture is not whole cells across and down, or a cell lies outside it or repeats.
    /// </exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, IEnumerable<RoomBackgroundCell> cells)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteSheetFrame picture = Ensure.NotNull(sheet).Frame(FrameName);
        if (picture.Size.X == 0 || picture.Size.X % CellPixels != 0 || picture.Size.Y == 0 || picture.Size.Y % CellPixels != 0)
            throw new InvalidDataException($"Room background {name} is {picture.Size.X}x{picture.Size.Y}, not whole cells across and down.");
        Name = name;
        Sheet = sheet;
        Picture = picture;
        CellCount = picture.Size.X / CellPixels * (picture.Size.Y / CellPixels);
        Cells = [.. Ensure.NotNull(cells)];
        if (Cells.Any(cell => cell.Cell < 0 || cell.Cell >= CellCount) || Cells.DistinctBy(cell => cell.Cell).Count() != Cells.Count)
            throw new InvalidDataException($"Room background {name} records a changing cell outside its {CellCount} cells, or one twice.");
    }

    /// <summary>The background's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The picture of the whole background.</summary>
    public SpriteSheetFrame Picture { get; }

    /// <summary>The image's 16x16 cells, row by row across the picture.</summary>
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
