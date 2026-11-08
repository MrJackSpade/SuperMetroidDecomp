using System.Collections.Immutable;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A Ceres perspective floor a gameplay frame shows in place of the room's ordinary layers (#1407): the elevator shaft's
/// turning floor, or the floor Ceres Ridley's getaway flies over, below which the room's background shows and around
/// which the chamber's walls stand (#1545).
/// </summary>
/// <param name="Transform">How the floor appears on the screen before room shake.</param>
/// <param name="Below">What shows in a band below the floor.</param>
/// <param name="Scenery">The pictures fixed in the room behind the floor, in drawing order; none when default or empty.</param>
public readonly record struct PerspectiveFloor(FloorView Transform, BelowPerspectiveFloor Below,
    ImmutableArray<PerspectiveFloorScenery> Scenery = default);
