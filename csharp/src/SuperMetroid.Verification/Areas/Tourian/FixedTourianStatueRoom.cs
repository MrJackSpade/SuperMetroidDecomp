using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>A Tourian statue room a focused check holds still: its water surface and the entrance statue's offset.</summary>
/// <param name="WaterSurface">The room line of the water surface, or null for a room effect that names none.</param>
/// <param name="StatueOffset">The entrance statue's vertical offset: minus the whole pixels it has sunk.</param>
internal sealed record FixedTourianStatueRoom(int WaterSurface, int StatueOffset) : ITourianStatueRoom;
