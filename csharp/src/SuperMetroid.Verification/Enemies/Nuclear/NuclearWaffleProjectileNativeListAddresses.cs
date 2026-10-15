using SuperMetroid.Core.Enemies.Nuclear;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>
/// The native bank-$86 lists the Nuclear Waffle body ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class NuclearWaffleProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (NuclearWaffleProjectileProgram Program, ushort Entry)[] Entries =
    [
        (NuclearWaffleProjectileProgram.Initial, 0xbb5e),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:BB5E-BB91, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xbb5e, 0xbb92);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(NuclearWaffleProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
