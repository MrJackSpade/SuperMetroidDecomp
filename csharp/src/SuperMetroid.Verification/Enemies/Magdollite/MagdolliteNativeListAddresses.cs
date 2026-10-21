using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<MagdolliteProgram>[] Entries =
    [
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftIdle, 0xac9c),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftThrow, 0xacb0),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftSubmerge, 0xacde),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftRaisePillar, 0xacfe),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftEmerge, 0xad0c),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.LeftLowerPillar, 0xad14),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightIdle, 0xad3c),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightThrow, 0xad50),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightSubmerge, 0xad7e),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightRaisePillar, 0xad9e),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightEmerge, 0xadac),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.RightLowerPillar, 0xadb4),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase0, 0xaddc),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase1, 0xade2),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase2, 0xade8),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase3, 0xadee),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase4, 0xadf4),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase5, 0xadfa),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase6, 0xae00),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarPhase7, 0xae06),
        new NativeProgramEntry<MagdolliteProgram>(MagdolliteProgram.PillarCap, 0xae0c),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MagdolliteNativeListAddressesInstruction> Instructions = new()
    {
        [0xae12] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.PlaySoundIfOnScreen, "V"),
        [0xae26] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.MoveDown2, ""),
        [0xae30] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.MoveUp2, ""),
        [0xae3a] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.SetAnimationBusy, ""),
        [0xae45] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.ClearAnimationBusy, ""),
        [0xae50] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.MoveWithPillarUp1, ""),
        [0xae5a] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.MoveWithPillarDown1, ""),
        [0xae64] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.SinkAndShowParts, ""),
        [0xae88] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.RestoreOrigins, ""),
        [0xae96] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.SurfaceAndHideParts, ""),
        [0xaeba] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.SpawnLava, ""),
        [0xaeca] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.ShiftRightUp4, ""),
        [0xaee4] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.ShiftLeftUp4, ""),
        [0xaefe] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.ShiftRightUp8, ""),
        [0xaf18] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.ShiftLeftUp4Again, ""),
        [0xaf44] = new MagdolliteNativeListAddressesInstruction(MagdolliteCueKind.StartCooldown, ""),
    };
}
