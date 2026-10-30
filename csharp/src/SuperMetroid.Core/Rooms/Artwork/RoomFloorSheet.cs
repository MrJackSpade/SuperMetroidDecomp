using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed turning floor of one Ceres tileset (#1392, #1467): the whole floor a room of the tileset starts from, and
/// a picture of every state gameplay draws over part of it, each under the name gameplay draws it by and placed by its
/// origin. The import composes them once, coloured with the tileset's standard palette and the sprite half as a room load
/// leaves it; the pictures of one part of the floor share its place, and no two parts overlap.
/// </summary>
public sealed class RoomFloorSheet
{
    /// <summary>The frame picturing the whole floor a room starts from.</summary>
    public const string FloorFrameName = "floor";

    /// <summary>Checks the sheet pictures the whole floor.</summary>
    /// <exception cref="InvalidDataException">The floor's frame is not the whole floor or not coloured from the full palette.</exception>
    public RoomFloorSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Floor = new AffineFloor(sheet.Frame(FloorFrameName));
    }

    /// <summary>The Ceres tileset.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The floor a room of the tileset starts from, with nothing drawn over it.</summary>
    public AffineFloor Floor { get; }

    /// <summary>The picture gameplay draws over the floor under <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet holds no such picture.</exception>
    public SpriteSheetFrame Picture(string name) => Sheet.Frame(name);

    /// <summary>The installed name of <paramref name="tileset"/>'s floor.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Floor.{RoomBlockSheet.NameOf(tileset)}";
}
