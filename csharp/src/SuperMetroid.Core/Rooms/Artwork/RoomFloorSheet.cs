using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed turning floor of one tileset whose rooms show one (#1392, #1467, #1649, #2104, #2573): the whole floor, which
/// shows nothing where the actors shown on it stand, and a picture of every one of those actors' pictures, placed by its
/// origin and drawn by the frame (#1536) with the floor's transform over it. The import composes them once, coloured with the
/// tileset's standard colours and the actors' colours as a room load leaves them; the pictures of one actor share its place,
/// and no two actors overlap. A tileset whose rooms show no turning floor installs none.
/// </summary>
public sealed partial class RoomFloorSheet
{
    /// <summary>The frame picturing the whole floor a room starts from.</summary>
    public const string FloorFrameName = "floor";

    /// <summary>The picture of each floor animation frame, by the frame.</summary>
    private readonly Dictionary<CeresFloorActorPicture, SpriteSheetFrame> pictures;

    /// <summary>Checks the sheet pictures the floor and every floor animation frame (#1536).</summary>
    /// <exception cref="InvalidDataException">
    /// The floor's frame is not the whole floor, or the sheet pictures a floor animation frame under no frame of its name.
    /// </exception>
    public RoomFloorSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
        Floor = new AffineFloor(sheet.Frame(FloorFrameName));
        pictures = Enum.GetValues<CeresFloorActorPicture>().ToDictionary(frame => frame, frame => sheet.Frame(PictureNameOf(frame)));
        Resting = Enum.GetValues<CeresFloorActorPicture>().Where(frame => sheet.HoldsPicture(RestingNameOf(frame)))
            .ToDictionary(CeresFloorActors.Of, frame => frame);
    }

    /// <summary>
    /// The picture each actor rests in on this floor as a room loads (#2573), where the floor has it rest; the floor shows
    /// nothing where a resting actor stands, and its own look where an actor that does not rest on it would.
    /// </summary>
    public IReadOnlyDictionary<CeresFloorActor, CeresFloorActorPicture> Resting { get; }

    /// <summary>
    /// The name under which a floor's sheet marks <paramref name="frame"/> as the picture its actor rests in on the floor
    /// (#2573), the picture <paramref name="frame"/> shows.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a picture of the floor's actors.</exception>
    [InstalledKey]
    public static string RestingNameOf(CeresFloorActorPicture frame) => Enum.IsDefined(frame)
        ? $"Ceres.Resting.{CeresFloorActorPictureKeys.Installed.Of(frame)}"
        : throw new InvalidDataException($"Unknown Ceres floor actor picture {frame}.");

    /// <summary>The Ceres tileset.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The sheet.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The floor alone, with no actor drawn over it.</summary>
    public AffineFloor Floor { get; }

    /// <summary>The picture <paramref name="frame"/>, which its actor shows over the floor.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public SpriteSheetFrame Picture(CeresFloorActorPicture frame) =>
        pictures.TryGetValue(frame, out SpriteSheetFrame? picture) ? picture
            : throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");

    /// <summary>The installed name of <paramref name="tileset"/>'s floor.</summary>
    [InstalledKey]
    public static string NameOf(TilesetDefinition tileset) => $"Floor.{RoomBlockSheet.NameOf(tileset)}";

    /// <summary>The name <paramref name="frame"/>'s picture is installed under in a floor's sheet.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not a floor animation frame.</exception>
    [InstalledKey]
    public static string PictureNameOf(CeresFloorActorPicture frame) => Enum.IsDefined(frame)
        ? $"Ceres.{CeresFloorActorPictureKeys.Installed.Of(frame)}"
        : throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");
}
