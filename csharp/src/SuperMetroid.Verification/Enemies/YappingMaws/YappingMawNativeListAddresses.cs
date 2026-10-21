using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<YappingMawProgram>[] Entries =
    [
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingUp, 0x9f6f),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingUpRight, 0x9f85),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingRight, 0x9f9b),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingDownRight, 0x9fb1),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingDown, 0x9fc7),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingDownLeft, 0x9fdd),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingLeft, 0x9ff3),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.AttackingUpLeft, 0xa009),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownUpRight, 0xa01f),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownUp, 0xa025),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownUpLoop, 0xa027),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownUpLeft, 0xa03d),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownUpLeftLoop, 0xa045),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownDownRight, 0xa05b),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownDown, 0xa061),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownDownLoop, 0xa063),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownDownLeft, 0xa079),
        new NativeProgramEntry<YappingMawProgram>(YappingMawProgram.CooldownDownLeftLoop, 0xa081),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, YappingMawNativeListAddressesInstruction> Instructions = new()
    {
        [0xa0c7] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusUpRight, ""),
        [0xa0d9] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusUpLeft, ""),
        [0xa0eb] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusDownRight, ""),
        [0xa0fd] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusDownLeft, ""),
        [0xa10f] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusUp, ""),
        [0xa121] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.HoldSamusDown, ""),
        [0xa133] = new YappingMawNativeListAddressesInstruction(YappingMawCueKind.PlayBiteSound, ""),
    };
}
