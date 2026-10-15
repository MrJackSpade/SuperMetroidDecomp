using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// The native bank-$A8 lists the Magdollite ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MagdolliteNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (MagdolliteProgram Program, ushort Entry)[] Entries =
    [
        (MagdolliteProgram.LeftIdle, 0xac9c),
        (MagdolliteProgram.LeftThrow, 0xacb0),
        (MagdolliteProgram.LeftSubmerge, 0xacde),
        (MagdolliteProgram.LeftRaisePillar, 0xacfe),
        (MagdolliteProgram.LeftEmerge, 0xad0c),
        (MagdolliteProgram.LeftLowerPillar, 0xad14),
        (MagdolliteProgram.RightIdle, 0xad3c),
        (MagdolliteProgram.RightThrow, 0xad50),
        (MagdolliteProgram.RightSubmerge, 0xad7e),
        (MagdolliteProgram.RightRaisePillar, 0xad9e),
        (MagdolliteProgram.RightEmerge, 0xadac),
        (MagdolliteProgram.RightLowerPillar, 0xadb4),
        (MagdolliteProgram.PillarPhase0, 0xaddc),
        (MagdolliteProgram.PillarPhase1, 0xade2),
        (MagdolliteProgram.PillarPhase2, 0xade8),
        (MagdolliteProgram.PillarPhase3, 0xadee),
        (MagdolliteProgram.PillarPhase4, 0xadf4),
        (MagdolliteProgram.PillarPhase5, 0xadfa),
        (MagdolliteProgram.PillarPhase6, 0xae00),
        (MagdolliteProgram.PillarPhase7, 0xae06),
        (MagdolliteProgram.PillarCap, 0xae0c),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (MagdolliteCueKind Kind, string Operands)> Instructions = new()
    {
        [0xae12] = (MagdolliteCueKind.PlaySoundIfOnScreen, "V"),
        [0xae26] = (MagdolliteCueKind.MoveDown2, ""),
        [0xae30] = (MagdolliteCueKind.MoveUp2, ""),
        [0xae3a] = (MagdolliteCueKind.SetAnimationBusy, ""),
        [0xae45] = (MagdolliteCueKind.ClearAnimationBusy, ""),
        [0xae50] = (MagdolliteCueKind.MoveWithPillarUp1, ""),
        [0xae5a] = (MagdolliteCueKind.MoveWithPillarDown1, ""),
        [0xae64] = (MagdolliteCueKind.SinkAndShowParts, ""),
        [0xae88] = (MagdolliteCueKind.RestoreOrigins, ""),
        [0xae96] = (MagdolliteCueKind.SurfaceAndHideParts, ""),
        [0xaeba] = (MagdolliteCueKind.SpawnLava, ""),
        [0xaeca] = (MagdolliteCueKind.ShiftRightUp4, ""),
        [0xaee4] = (MagdolliteCueKind.ShiftLeftUp4, ""),
        [0xaefe] = (MagdolliteCueKind.ShiftRightUp8, ""),
        [0xaf18] = (MagdolliteCueKind.ShiftLeftUp4Again, ""),
        [0xaf44] = (MagdolliteCueKind.StartCooldown, ""),
    };
}
