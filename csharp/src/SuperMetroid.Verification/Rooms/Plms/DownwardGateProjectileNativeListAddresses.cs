using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// The native bank-$86 lists the downward gate ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class DownwardGateProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (DownwardGateProjectileProgram Program, ushort Entry)[] Entries =
    [
        (DownwardGateProjectileProgram.Moving, 0xe53c),
        (DownwardGateProjectileProgram.MovingStep2, 0xe54a),
        (DownwardGateProjectileProgram.MovingStep3, 0xe550),
        (DownwardGateProjectileProgram.MovingStep4, 0xe556),
        (DownwardGateProjectileProgram.MovingStep5, 0xe55c),
        (DownwardGateProjectileProgram.Closed, 0xe55e),
        (DownwardGateProjectileProgram.ClosedStep2, 0xe568),
        (DownwardGateProjectileProgram.ClosedStep3, 0xe572),
        (DownwardGateProjectileProgram.ClosedStep4, 0xe578),
        (DownwardGateProjectileProgram.ClosedStep5, 0xe57e),
        (DownwardGateProjectileProgram.ClosedStep6, 0xe584),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E53C-E585, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xe53c, 0xe586);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(DownwardGateProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
