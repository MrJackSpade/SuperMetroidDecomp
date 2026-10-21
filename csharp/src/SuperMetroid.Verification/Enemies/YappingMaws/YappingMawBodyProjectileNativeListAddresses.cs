using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// The native bank-$86 lists the Yapping Maw body link ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class YappingMawBodyProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<YappingMawBodyProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<YappingMawBodyProjectileProgram>(YappingMawBodyProjectileProgram.FacingDown, 0xec56),
        new NativeProgramEntry<YappingMawBodyProjectileProgram>(YappingMawBodyProjectileProgram.FacingUp, 0xec5c),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:EC56-EC61, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xec56, 0xec62);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(YappingMawBodyProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
