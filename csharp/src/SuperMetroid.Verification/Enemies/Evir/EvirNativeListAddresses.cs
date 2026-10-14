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
    internal static readonly (EvirProgram Program, ushort Entry)[] Entries =
    [
        (EvirProgram.BodyFacingLeft, 0x86a7),
        (EvirProgram.ArmsFacingLeft, 0x86c3),
        (EvirProgram.BodyFacingRight, 0x870b),
        (EvirProgram.ArmsFacingRight, 0x8727),
        (EvirProgram.ProjectileNormal, 0x876f),
        (EvirProgram.ProjectileRegenerating, 0x8775),
        (EvirProgram.ProjectileRegenerationLoop, 0x877d),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (EvirCueKind Kind, string Operands)> Instructions = new()
    {
        [0x878f] = (EvirCueKind.PlaySpitSound, ""),
        [0x879b] = (EvirCueKind.SetInitialRegenerationOffset, ""),
        [0x87b6] = (EvirCueKind.AdvanceRegenerationOffset, ""),
        [0x87cb] = (EvirCueKind.FinishRegeneration, ""),
    };
}
