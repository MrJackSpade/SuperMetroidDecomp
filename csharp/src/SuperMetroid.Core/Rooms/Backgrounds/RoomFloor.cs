using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The turning floor a Ceres room shows (#1349, #1467, #1626, #2573): its tileset's installed floor, which holds only the
/// floor, and the picture each actor shown on it shows now, the elevator platform, the Baby's capsule and Ridley's wings,
/// drawn with the floor's transform over it. An actor that has shown no other picture yet shows its resting picture
/// (<see cref="CeresFloorActors.RestingPictures"/>). Room state; each frame the display composes the floor from it as an
/// <see cref="AffineFloor"/>.
/// </summary>
public sealed class RoomFloor
{
    /// <summary>The picture each actor on the floor shows now.</summary>
    private readonly Dictionary<CeresFloorActor, CeresFloorActorPicture> shown = new(CeresFloorActors.RestingPictures);

    /// <summary>The floor as composed, until a part shows another frame.</summary>
    private AffineFloor? published;

    /// <summary>Starts from the tileset's floor with every actor on it showing its resting picture.</summary>
    /// <param name="sheet">The tileset's installed floor and the pictures of its parts' frames.</param>
    public RoomFloor(RoomFloorSheet sheet) => Sheet = Ensure.NotNull(sheet);

    /// <summary>The tileset's installed floor and the pictures of the actors shown on it.</summary>
    public RoomFloorSheet Sheet { get; }

    /// <summary>The actor <paramref name="frame"/> is a picture of shows it from now on.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a picture of the floor's actors.</exception>
    public void Show(CeresFloorActorPicture frame)
    {
        CeresFloorActor part = CeresFloorActors.Of(frame);
        if (shown.TryGetValue(part, out CeresFloorActorPicture current) && current == frame) return;
        shown[part] = frame;
        published = null;
    }

    /// <summary>The floor the next frame shows, every actor's picture over the installed floor; shared until an actor changes.</summary>
    public AffineFloor Publish() => published ??= new AffineFloor(Sheet.Floor.Image,
        [.. Enum.GetValues<CeresFloorActor>().Select(actor => Sheet.Picture(shown[actor]))]);
}
