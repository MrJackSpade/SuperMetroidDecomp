using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Hibashi;

namespace SuperMetroid.Verification.Enemies.Hibashi;

/// <summary>
/// The native bank-$A6 lists the Hibashi ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class HibashiNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<HibashiSegment>[] Entries =
    [
        new NativeProgramEntry<HibashiSegment>(HibashiSegment.Graphics, 0x8d1b),
        new NativeProgramEntry<HibashiSegment>(HibashiSegment.Hitbox, 0x8da9),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, HibashiNativeListAddressesInstruction> Instructions = new()
    {
        [0x8daf] = new HibashiNativeListAddressesInstruction(HibashiCueKind.PlayEruptionSound, "", 0),
        [0x8e13] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 0),
        [0x8e2d] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 1),
        [0x8e41] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 2),
        [0x8e55] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 3),
        [0x8e69] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 4),
        [0x8e7d] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 5),
        [0x8e91] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 6),
        [0x8ea5] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 7),
        [0x8eb9] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 8),
        [0x8ecd] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 9),
        [0x8ee1] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 10),
        [0x8ef5] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 11),
        [0x8f09] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 12),
        [0x8f1d] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 13),
        [0x8f31] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 14),
        [0x8f45] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 15),
        [0x8f59] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 16),
        [0x8f6d] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 17),
        [0x8f81] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 18),
        [0x8f95] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 19),
        [0x8fa9] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 20),
        [0x8fbd] = new HibashiNativeListAddressesInstruction(HibashiCueKind.EruptionStep, "", 21),
        [0x8fd1] = new HibashiNativeListAddressesInstruction(HibashiCueKind.FinishActivity, "", 0),
    };
}
