namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed placement and rotation policy for one of the twelve room turrets.</summary>
/// <param name="Position">The turret's fixed room-pixel position.</param>
/// <param name="AllowedRotationPointer">The bank-$86 allowed-direction row.</param>
/// <param name="InitialDirection">The direction the turret starts in.</param>
/// <param name="AllowedDirectionMask">The directions the turret may rotate to, one bit each.</param>
internal readonly record struct MotherBrainTurretDefinition(
    Position16<RoomPixels> Position,
    ushort AllowedRotationPointer,
    MotherBrainTurretDirection InitialDirection,
    byte AllowedDirectionMask);
