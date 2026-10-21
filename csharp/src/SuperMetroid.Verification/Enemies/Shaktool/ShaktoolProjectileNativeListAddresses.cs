using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>
/// The native bank-$86 lists the Shaktool attack circle ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class ShaktoolProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ShaktoolProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.Front, 0xbd68),
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.FrontLoop, 0xbd70),
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.Middle, 0xbd78),
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.MiddleLoop, 0xbd84),
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.Back, 0xbd8c),
        new NativeProgramEntry<ShaktoolProjectileProgram>(ShaktoolProjectileProgram.BackLoop, 0xbd94),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:BD68-BD9B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xbd68, 0xbd9c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(ShaktoolProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
