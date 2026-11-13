namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One Torizo launch tuple whose two velocities receive independent signed random
/// displacements when the projectile is initialized, and the program it starts. The record
/// holds X offset, X velocity, Y offset and Y velocity.
/// </summary>
internal readonly record struct TorizoRandomizedProjectileDefinition(
    TorizoProjectileProgram Program,
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> BaseVelocity);
