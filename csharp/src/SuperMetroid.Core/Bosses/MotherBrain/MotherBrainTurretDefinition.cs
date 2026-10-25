namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed placement and rotation policy for one of the twelve room turrets.</summary>
/// <param name="Position">The turret's fixed room-pixel position.</param>
/// <param name="InitialDirection">The direction the turret starts in.</param>
/// <param name="FirstAllowedDirection">The first direction, in ring order, of the arc the turret may rotate through.</param>
/// <param name="LastAllowedDirection">The last direction, in ring order, of the arc the turret may rotate through.</param>
internal readonly record struct MotherBrainTurretDefinition(
    Position16<RoomPixels> Position,
    MotherBrainTurretDirection InitialDirection,
    MotherBrainTurretDirection FirstAllowedDirection,
    MotherBrainTurretDirection LastAllowedDirection);
