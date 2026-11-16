using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<MagdolliteSegment>[] Entries =
    [
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftIdle, 0xac9c),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftThrow, 0xacb0),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftSubmerge, 0xacde),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftRaisePillar, 0xacfe),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftEmerge, 0xad0c),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.LeftLowerPillar, 0xad14),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightIdle, 0xad3c),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightThrow, 0xad50),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightSubmerge, 0xad7e),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightRaisePillar, 0xad9e),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightEmerge, 0xadac),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.RightLowerPillar, 0xadb4),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.OneSegmentPillar, 0xaddc),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.TwoSegmentPillar, 0xade2),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.ThreeSegmentPillar, 0xade8),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.FourSegmentPillar, 0xadee),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.FiveSegmentPillar, 0xadf4),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.SixSegmentPillar, 0xadfa),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.SevenSegmentPillar, 0xae00),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.EightSegmentPillar, 0xae06),
        new NativeProgramEntry<MagdolliteSegment>(MagdolliteSegment.PillarCap, 0xae0c),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MagdolliteNativeListAddressesInstruction> Instructions = new()
    {
        [0xae12] = new MagdolliteNativeListAddressesInstruction(null, "V"),
        [0xae26] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Move(new(0, 2)), ""),
        [0xae30] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Move(new(0, -2)), ""),
        [0xae3a] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.SetAnimationBusy), ""),
        [0xae45] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.ClearAnimationBusy), ""),
        [0xae50] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.MoveWithPillar(new(0, -1)), ""),
        [0xae5a] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.MoveWithPillar(new(0, 1)), ""),
        [0xae64] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.SinkAndShowParts), ""),
        [0xae88] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.RestoreOrigins), ""),
        [0xae96] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.SurfaceAndHideParts), ""),
        [0xaeba] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.SpawnLava), ""),
        [0xaeca] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.ShiftFromThrowOrigin(new(8, -4)), ""),
        [0xaee4] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.ShiftFromThrowOrigin(new(-8, -4)), ""),
        [0xaefe] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.ShiftFromThrowOrigin(new(8, -8)), ""),
        [0xaf18] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.ShiftFromThrowOrigin(new(-8, -4)), ""),
        [0xaf44] = new MagdolliteNativeListAddressesInstruction(new MagdolliteCue.Signal(MagdolliteCueKind.StartCooldown), ""),
    };
}
