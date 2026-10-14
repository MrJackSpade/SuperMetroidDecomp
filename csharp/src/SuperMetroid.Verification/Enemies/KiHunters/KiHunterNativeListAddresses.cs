using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// The native bank-$A8 lists the Ki-Hunter ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class KiHunterNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (KiHunterProgram Program, ushort Entry)[] Entries =
    [
        (KiHunterProgram.FlyingLeft, 0xe9fa),
        (KiHunterProgram.SwoopLeft, 0xea08),
        (KiHunterProgram.FlyingRight, 0xea24),
        (KiHunterProgram.SwoopRight, 0xea32),
        (KiHunterProgram.WingsLeft, 0xea4e),
        (KiHunterProgram.WingsRight, 0xea5e),
        (KiHunterProgram.DetachedWings, 0xea7e),
        (KiHunterProgram.JumpLeft, 0xea8a),
        (KiHunterProgram.JumpRight, 0xeaa6),
        (KiHunterProgram.LandLeft, 0xeac2),
        (KiHunterProgram.LandRight, 0xeada),
        (KiHunterProgram.SpitLeft, 0xeaf2),
        (KiHunterProgram.SpitRight, 0xeb10),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (KiHunterCueKind Kind, string Operands)> Instructions = new()
    {
        [0xf526] = (KiHunterCueKind.ReturnToFlying, ""),
        [0xf5e4] = (KiHunterCueKind.StartHop, ""),
        [0xf67f] = (KiHunterCueKind.StartWinglessThinking, ""),
        [0xf6d2] = (KiHunterCueKind.SpitLeft, ""),
        [0xf6d8] = (KiHunterCueKind.SpitRight, ""),
    };
}
