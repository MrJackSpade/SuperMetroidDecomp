namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A Ceres perspective floor a gameplay frame shows in place of the room's ordinary layers (#1407): the elevator shaft's
/// turning floor, or the floor Ceres Ridley's getaway flies over, below which the room's layer 2 shows.
/// </summary>
/// <param name="Transform">The floor's transform before room shake.</param>
/// <param name="ShowsLayer2Below">Whether the room's layer 2 shows in a band below the floor, as in Ridley's getaway.</param>
public readonly record struct PerspectiveFloor(AffineTransform Transform, bool ShowsLayer2Below);
