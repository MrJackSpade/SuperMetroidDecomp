using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The turning floor a Ceres room shows (#1349, #1467, #1626): its tileset's installed floor and the frame each of its
/// animated parts shows now, the elevator platform, the Baby's capsule and Ridley's wings. A part no animation has shown
/// a frame of yet shows as the installed floor pictures it. Room state; each frame the display composes the floor from
/// it as an <see cref="AffineFloor"/>.
/// </summary>
public sealed class RoomFloor
{
    /// <summary>The frame each animated part shows now; a part absent from it shows as the installed floor pictures it.</summary>
    internal readonly Dictionary<CeresFloorPart, CeresFloorAnimationFrame> shown = [];

    /// <summary>The floor as composed, until a part shows another frame.</summary>
    private AffineFloor? published;

    /// <summary>Starts from the tileset's floor with every part as the floor pictures it.</summary>
    /// <param name="sheet">The tileset's installed floor and the pictures of its parts' frames.</param>
    public RoomFloor(RoomFloorSheet sheet) => Sheet = Ensure.NotNull(sheet);

    /// <summary>The tileset's installed floor and the pictures of its parts' frames.</summary>
    public RoomFloorSheet Sheet { get; }

    /// <summary>Shows <paramref name="frame"/> on the part of the floor it animates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public void Show(CeresFloorAnimationFrame frame)
    {
        CeresFloorPart part = CeresFloorParts.Of(frame);
        if (shown.TryGetValue(part, out CeresFloorAnimationFrame current) && current == frame) return;
        shown[part] = frame;
        published = null;
    }

    /// <summary>The floor the next frame shows, its parts' frames over the installed floor; shared until a part changes.</summary>
    public AffineFloor Publish() => published ??= new AffineFloor(Sheet.Floor.Image,
        [.. Enum.GetValues<CeresFloorPart>().Where(shown.ContainsKey).Select(part => Sheet.Picture(shown[part]))]);
}
