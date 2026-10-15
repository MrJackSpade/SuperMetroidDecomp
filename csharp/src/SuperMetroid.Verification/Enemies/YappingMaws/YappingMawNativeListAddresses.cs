using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// The native bank-$A8 lists the Yapping Maw ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class YappingMawNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (YappingMawProgram Program, ushort Entry)[] Entries =
    [
        (YappingMawProgram.AttackingUp, 0x9f6f),
        (YappingMawProgram.AttackingUpRight, 0x9f85),
        (YappingMawProgram.AttackingRight, 0x9f9b),
        (YappingMawProgram.AttackingDownRight, 0x9fb1),
        (YappingMawProgram.AttackingDown, 0x9fc7),
        (YappingMawProgram.AttackingDownLeft, 0x9fdd),
        (YappingMawProgram.AttackingLeft, 0x9ff3),
        (YappingMawProgram.AttackingUpLeft, 0xa009),
        (YappingMawProgram.CooldownUpRight, 0xa01f),
        (YappingMawProgram.CooldownUp, 0xa025),
        (YappingMawProgram.CooldownUpLoop, 0xa027),
        (YappingMawProgram.CooldownUpLeft, 0xa03d),
        (YappingMawProgram.CooldownUpLeftLoop, 0xa045),
        (YappingMawProgram.CooldownDownRight, 0xa05b),
        (YappingMawProgram.CooldownDown, 0xa061),
        (YappingMawProgram.CooldownDownLoop, 0xa063),
        (YappingMawProgram.CooldownDownLeft, 0xa079),
        (YappingMawProgram.CooldownDownLeftLoop, 0xa081),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (YappingMawCueKind Kind, string Operands)> Instructions = new()
    {
        [0xa0c7] = (YappingMawCueKind.HoldSamusUpRight, ""),
        [0xa0d9] = (YappingMawCueKind.HoldSamusUpLeft, ""),
        [0xa0eb] = (YappingMawCueKind.HoldSamusDownRight, ""),
        [0xa0fd] = (YappingMawCueKind.HoldSamusDownLeft, ""),
        [0xa10f] = (YappingMawCueKind.HoldSamusUp, ""),
        [0xa121] = (YappingMawCueKind.HoldSamusDown, ""),
        [0xa133] = (YappingMawCueKind.PlayBiteSound, ""),
    };
}
