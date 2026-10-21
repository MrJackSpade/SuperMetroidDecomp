using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Dragon;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>
/// The native bank-$A2 lists the Dragon ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class DragonNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DragonProgram>[] Entries =
    [
        new NativeProgramEntry<DragonProgram>(DragonProgram.IdleFacingLeft, 0xe59b),
        new NativeProgramEntry<DragonProgram>(DragonProgram.WingsFacingLeft, 0xe5a1),
        new NativeProgramEntry<DragonProgram>(DragonProgram.IdleFacingRight, 0xe5ad),
        new NativeProgramEntry<DragonProgram>(DragonProgram.WingsFacingRight, 0xe5b3),
        new NativeProgramEntry<DragonProgram>(DragonProgram.AttackingFacingLeft, 0xe5bf),
        new NativeProgramEntry<DragonProgram>(DragonProgram.AttackingFacingRight, 0xe5d7),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, DragonNativeListAddressesInstruction> Instructions = new()
    {
        [0xe5fb] = new DragonNativeListAddressesInstruction(DragonCueKind.FinishAttack, ""),
    };
}
