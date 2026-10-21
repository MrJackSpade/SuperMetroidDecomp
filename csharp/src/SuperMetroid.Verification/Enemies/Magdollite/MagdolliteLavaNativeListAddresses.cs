using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// The native bank-$86 lists the Magdollite lava ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MagdolliteLavaNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MagdolliteLavaProgram>[] Entries =
    [
        new NativeProgramEntry<MagdolliteLavaProgram>(MagdolliteLavaProgram.SharedDelete, 0x84fc),
        new NativeProgramEntry<MagdolliteLavaProgram>(MagdolliteLavaProgram.Left, 0xdfd8),
        new NativeProgramEntry<MagdolliteLavaProgram>(MagdolliteLavaProgram.Right, 0xdfde),
        new NativeProgramEntry<MagdolliteLavaProgram>(MagdolliteLavaProgram.Shot, 0xdfe4),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:DFD8-DFE9, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xdfd8, 0xdfea);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MagdolliteLavaProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
