using SuperMetroid.Core.Enemies.Stoke;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>
/// The native bank-$86 lists the Stoke projectile ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class StokeProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (StokeProjectileProgram Program, ushort Entry)[] Entries =
    [
        (StokeProjectileProgram.Initial, 0xdb0c),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:DB0C-DB17, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xdb0c, 0xdb18);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(StokeProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
