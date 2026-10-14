namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One bank-$86 Torizo launch tuple whose two velocity words receive independent signed
/// random-byte displacements when the projectile is initialized, and the program it starts. The native record
/// interleaves X offset, X velocity, Y offset and Y velocity words.
/// </summary>
internal readonly record struct TorizoRandomizedProjectileDefinition(
    TorizoProjectileProgram Program,
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> BaseVelocity);
