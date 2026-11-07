using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed turning floor of one Ceres tileset (#1392, #1467): the whole floor a room of the tileset starts from, and
/// a picture of every floor animation frame gameplay draws over part of it, placed by its origin and drawn by the
/// frame (#1536). The import composes them once, coloured with the tileset's standard palette and the sprite half as a room load
/// leaves it; the pictures of one part of the floor share its place, and no two parts overlap.
/// </summary>
public sealed class RoomFloorSheet
{
    /// <summary>The frame picturing the whole floor a room starts from.</summary>
    public const string FloorFrameName = "floor";

    /// <summary>The picture of each floor animation frame, by the frame.</summary>
    private readonly Dictionary<CeresFloorAnimationFrame, SpriteSheetFrame> pictures;

    /// <summary>Checks the sheet pictures the whole floor and every floor animation frame (#1536).</summary>
    /// <exception cref="InvalidDataException">
    /// The floor's frame is not the whole floor or not coloured from the full palette, or the sheet pictures a floor
    /// animation frame under no frame of its name.
    /// </exception>
    public RoomFloorSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Floor = new AffineFloor(sheet.Frame(FloorFrameName));
        pictures = Enum.GetValues<CeresFloorAnimationFrame>().ToDictionary(frame => frame, frame => sheet.Frame(PictureNameOf(frame)));
    }

    /// <summary>The Ceres tileset.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The floor a room of the tileset starts from, with nothing drawn over it.</summary>
    public AffineFloor Floor { get; }

    /// <summary>The picture of floor animation frame <paramref name="frame"/>, which gameplay draws over the floor.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public SpriteSheetFrame Picture(CeresFloorAnimationFrame frame) =>
        pictures.TryGetValue(frame, out SpriteSheetFrame? picture) ? picture
            : throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");

    /// <summary>The installed name of <paramref name="tileset"/>'s floor.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Floor.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>The name <paramref name="frame"/>'s picture is installed under in a floor's sheet.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public static string PictureNameOf(CeresFloorAnimationFrame frame) => Enum.IsDefined(frame)
        ? $"Ceres.{frame}"
        : throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");
}
