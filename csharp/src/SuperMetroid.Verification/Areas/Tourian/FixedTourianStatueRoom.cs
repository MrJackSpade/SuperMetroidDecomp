using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// A Tourian statue room a focused check holds still: its water surface and the entrance statue's offset, with the statues'
/// animation state starting as a room load starts it.
/// </summary>
/// <param name="WaterSurface">The room line of the water surface.</param>
/// <param name="StatueOffset">The entrance statue's vertical offset: minus the whole pixels it has sunk.</param>
internal sealed record FixedTourianStatueRoom(int WaterSurface, int StatueOffset) : ITourianStatueRoom
{
    /// <inheritdoc/>
    public TourianStatueAnimation Animation { get; } = new(new SuperMetroid.Core.Rooms.RoomObjects.TourianStatueProgress());
}
