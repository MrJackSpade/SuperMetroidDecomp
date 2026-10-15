using SuperMetroid.Core.Enemies.Kago;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>
/// The native bank-$86 lists the Kago bug ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class KagoBugProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (KagoBugProjectileProgram Program, ushort Entry)[] Entries =
    [
        (KagoBugProjectileProgram.SharedDelete, 0x84fc),
        (KagoBugProjectileProgram.Landed, 0xd03c),
        (KagoBugProjectileProgram.Falling, 0xd04a),
        (KagoBugProjectileProgram.JumpStart, 0xd052),
        (KagoBugProjectileProgram.JumpLoop, 0xd05c),
        (KagoBugProjectileProgram.Shot, 0xd064),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:D03C-D07F, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xd03c, 0xd080);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(KagoBugProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
