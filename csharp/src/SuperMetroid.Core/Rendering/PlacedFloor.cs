using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>A turning floor and how it appears on the screen (#2505): its map and pictures (#1403) with their view, together.</summary>
/// <param name="View">How the floor is turned, scaled and scrolled onto the screen.</param>
/// <param name="Map">The floor's map and pictures.</param>
public sealed record PlacedFloor(FloorView View, AffineFloor Map)
{
    /// <summary>The floor's map and pictures.</summary>
    public AffineFloor Map
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Map);
}
