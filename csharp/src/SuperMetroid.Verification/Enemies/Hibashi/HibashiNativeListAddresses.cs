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
    internal static readonly (HibashiProgram Program, ushort Entry)[] Entries =
    [
        (HibashiProgram.Graphics, 0x8d1b),
        (HibashiProgram.Hitbox, 0x8da9),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (HibashiCueKind Kind, string Operands, ushort Value)> Instructions = new()
    {
        [0x8daf] = (HibashiCueKind.PlayEruptionSound, "", 0),
        [0x8e13] = (HibashiCueKind.ActivityFrame, "", 0),
        [0x8e2d] = (HibashiCueKind.ActivityFrame, "", 1),
        [0x8e41] = (HibashiCueKind.ActivityFrame, "", 2),
        [0x8e55] = (HibashiCueKind.ActivityFrame, "", 3),
        [0x8e69] = (HibashiCueKind.ActivityFrame, "", 4),
        [0x8e7d] = (HibashiCueKind.ActivityFrame, "", 5),
        [0x8e91] = (HibashiCueKind.ActivityFrame, "", 6),
        [0x8ea5] = (HibashiCueKind.ActivityFrame, "", 7),
        [0x8eb9] = (HibashiCueKind.ActivityFrame, "", 8),
        [0x8ecd] = (HibashiCueKind.ActivityFrame, "", 9),
        [0x8ee1] = (HibashiCueKind.ActivityFrame, "", 10),
        [0x8ef5] = (HibashiCueKind.ActivityFrame, "", 11),
        [0x8f09] = (HibashiCueKind.ActivityFrame, "", 12),
        [0x8f1d] = (HibashiCueKind.ActivityFrame, "", 13),
        [0x8f31] = (HibashiCueKind.ActivityFrame, "", 14),
        [0x8f45] = (HibashiCueKind.ActivityFrame, "", 15),
        [0x8f59] = (HibashiCueKind.ActivityFrame, "", 16),
        [0x8f6d] = (HibashiCueKind.ActivityFrame, "", 17),
        [0x8f81] = (HibashiCueKind.ActivityFrame, "", 18),
        [0x8f95] = (HibashiCueKind.ActivityFrame, "", 19),
        [0x8fa9] = (HibashiCueKind.ActivityFrame, "", 20),
        [0x8fbd] = (HibashiCueKind.ActivityFrame, "", 21),
        [0x8fd1] = (HibashiCueKind.FinishActivity, "", 0),
    };
}
