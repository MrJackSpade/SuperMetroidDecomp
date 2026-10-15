using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// The native bank-$86 lists the eye door projectile ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EyeDoorProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (EyeDoorProjectileProgram Program, ushort Entry)[] Entries =
    [
        (EyeDoorProjectileProgram.Initial, 0xb5d9),
        (EyeDoorProjectileProgram.FlyingLoop, 0xb5eb),
        (EyeDoorProjectileProgram.Impact, 0xb5f3),
        (EyeDoorProjectileProgram.Shot, 0xb603),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:B5D9-B614, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xb5d9, 0xb615);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EyeDoorProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
