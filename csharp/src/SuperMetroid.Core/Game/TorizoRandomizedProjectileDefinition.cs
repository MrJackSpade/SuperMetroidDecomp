namespace SuperMetroid.Core.Game;

/// <summary>
/// One bank-$86 Torizo launch tuple whose two velocity words receive independent signed
/// random-byte displacements when the projectile is initialized. The native record interleaves
/// X offset, X velocity, Y offset and Y velocity words.
/// </summary>
internal readonly record struct TorizoRandomizedProjectileDefinition(
    ushort InstructionList,
    Offset16<RoomPixels> Offset,
    EightEightVector<RoomPixels> BaseVelocity);
