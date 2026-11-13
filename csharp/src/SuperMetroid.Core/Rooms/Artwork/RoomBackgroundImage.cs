using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// One installed pre-coloured room background (#1392, #1466, #1555): the picture a room shows on the background, coloured
/// with its own colours, and the cells whose pictures change with the room's appearance inputs. A library background
/// pictures one period of what it shows (#1651), laid out as its room shows it (#1617), which the background shows over and
/// over across and down; the scrolling sky pictures all its rows.
/// </summary>
public sealed class RoomBackgroundImage
{
    /// <summary>The frame of an image's sheet that pictures the whole background.</summary>
    public const string FrameName = "pages";

    /// <summary>Pixels along a cell's side.</summary>
    public const int CellPixels = 16;

    /// <summary>
    /// Checks the image is whole cells across and down, that each installed part of its picture (#1634) is the image's size,
    /// and that each changing cell lies in it once.
    /// </summary>
    /// <param name="name">The background's installed name.</param>
    /// <param name="sheet">The image's sheet, which installs the picture's parts as <see cref="FrameName"/>'s.</param>
    /// <param name="size">The image's width and height, which a part that shows nothing does not record.</param>
    /// <param name="cells">The cells whose pictures change with the room's appearance inputs.</param>
    /// <exception cref="InvalidDataException">
    /// The image is not whole cells across and down, a part of its picture is not its size, or a cell lies outside it or
    /// repeats.
    /// </exception>
    public RoomBackgroundImage(string name, SpriteSheet sheet, Extent16<ImagePixels> size, IEnumerable<RoomBackgroundCell> cells)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (size.X == 0 || size.X % CellPixels != 0 || size.Y == 0 || size.Y % CellPixels != 0)
            throw new InvalidDataException($"Room background {name} is {size.X}x{size.Y}, not whole cells across and down.");
        LayeredPicture picture = PictureParts.Of(Ensure.NotNull(sheet), FrameName);
        foreach (SpriteSheetFrame part in picture.Parts)
        {
            if (part.Size != size)
                throw new InvalidDataException($"Room background {name}'s picture has a {part.Size.X}x{part.Size.Y} part, not {size.X}x{size.Y}.");
        }
        Name = name;
        Sheet = sheet;
        Picture = picture;
        Size = size;
        CellCount = size.X / CellPixels * (size.Y / CellPixels);
        Cells = [.. Ensure.NotNull(cells)];
        if (Cells.Any(cell => cell.Cell < 0 || cell.Cell >= CellCount) || Cells.DistinctBy(cell => cell.Cell).Count() != Cells.Count)
            throw new InvalidDataException($"Room background {name} records a changing cell outside its {CellCount} cells, or one twice.");
    }

    /// <summary>The background's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>
    /// The picture of the whole background, as its part drawn behind the actors passing in front of the room's back tiles
    /// and its part drawn in front of them (#1634).
    /// </summary>
    public LayeredPicture Picture { get; }

    /// <summary>The image's width and height.</summary>
    public Extent16<ImagePixels> Size { get; }

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
