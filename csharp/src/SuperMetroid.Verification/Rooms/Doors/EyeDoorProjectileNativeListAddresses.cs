using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// The native bank-$86 lists the eye door projectile ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class EyeDoorProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EyeDoorProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<EyeDoorProjectileProgram>(EyeDoorProjectileProgram.Initial, 0xb5d9),
        new NativeProgramEntry<EyeDoorProjectileProgram>(EyeDoorProjectileProgram.FlyingLoop, 0xb5eb),
        new NativeProgramEntry<EyeDoorProjectileProgram>(EyeDoorProjectileProgram.Impact, 0xb5f3),
        new NativeProgramEntry<EyeDoorProjectileProgram>(EyeDoorProjectileProgram.Shot, 0xb603),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:B5D9-B614, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xb5d9, 0xb615);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(EyeDoorProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
