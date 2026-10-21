using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Stoke;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>
/// The native bank-$A2 lists the Stoke ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class StokeNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<StokeProgram>[] Entries =
    [
        new NativeProgramEntry<StokeProgram>(StokeProgram.MovingLeft, 0x8932),
        new NativeProgramEntry<StokeProgram>(StokeProgram.MovingLeftLoop, 0x8934),
        new NativeProgramEntry<StokeProgram>(StokeProgram.AttackingLeft, 0x8948),
        new NativeProgramEntry<StokeProgram>(StokeProgram.MovingRight, 0x8958),
        new NativeProgramEntry<StokeProgram>(StokeProgram.MovingRightLoop, 0x895a),
        new NativeProgramEntry<StokeProgram>(StokeProgram.AttackingRight, 0x896e),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, StokeNativeListAddressesInstruction> Instructions = new()
    {
        [0x897e] = new StokeNativeListAddressesInstruction(StokeCueKind.SpawnFireball, "V"),
        [0x8990] = new StokeNativeListAddressesInstruction(StokeCueKind.SetMovingLeft, ""),
        [0x899d] = new StokeNativeListAddressesInstruction(StokeCueKind.SetMovingRight, ""),
    };
}
