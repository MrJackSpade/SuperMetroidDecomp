using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Evir;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>
/// The native bank-$A8 lists the Evir ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class EvirNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<EvirProgram>[] Entries =
    [
        new NativeProgramEntry<EvirProgram>(EvirProgram.BodyFacingLeft, 0x86a7),
        new NativeProgramEntry<EvirProgram>(EvirProgram.ArmsFacingLeft, 0x86c3),
        new NativeProgramEntry<EvirProgram>(EvirProgram.BodyFacingRight, 0x870b),
        new NativeProgramEntry<EvirProgram>(EvirProgram.ArmsFacingRight, 0x8727),
        new NativeProgramEntry<EvirProgram>(EvirProgram.ProjectileNormal, 0x876f),
        new NativeProgramEntry<EvirProgram>(EvirProgram.ProjectileRegenerating, 0x8775),
        new NativeProgramEntry<EvirProgram>(EvirProgram.ProjectileRegenerationLoop, 0x877d),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, EvirNativeListAddressesInstruction> Instructions = new()
    {
        [0x878f] = new EvirNativeListAddressesInstruction(EvirCueKind.PlaySpitSound, ""),
        [0x879b] = new EvirNativeListAddressesInstruction(EvirCueKind.SetInitialRegenerationOffset, ""),
        [0x87b6] = new EvirNativeListAddressesInstruction(EvirCueKind.AdvanceRegenerationOffset, ""),
        [0x87cb] = new EvirNativeListAddressesInstruction(EvirCueKind.FinishRegeneration, ""),
    };
}
