using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>
/// The native bank-$AA lists the Chozo statue ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ChozoStatueNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (ChozoStatueProgram Program, ushort Entry)[] Entries =
    [
        (ChozoStatueProgram.LowerNorfairInitial, 0xe39d),
        (ChozoStatueProgram.LowerNorfairActivated, 0xe3a7),
        (ChozoStatueProgram.LowerNorfairActivatedLoop, 0xe3e5),
        (ChozoStatueProgram.WreckedShipInitial, 0xe457),
        (ChozoStatueProgram.WreckedShipActivated, 0xe461),
        (ChozoStatueProgram.WreckedShipActivatedLoop, 0xe49d),
        (ChozoStatueProgram.WreckedShipActivatedLoop2, 0xe4c1),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (ChozoStatueCueKind Kind, string Operands)> Instructions = new()
    {
        [0x806b] = (ChozoStatueCueKind.SetPreInstruction, "E"),
        [0x8074] = (ChozoStatueCueKind.ClearPreInstruction, ""),
        [0xe429] = (ChozoStatueCueKind.StartLoweringAcid, ""),
        [0xe436] = (ChozoStatueCueKind.SetLoweredAcidPosition, ""),
        [0xe43d] = (ChozoStatueCueKind.UnlockSamus, ""),
        [0xe57f] = (ChozoStatueCueKind.PlayGrabSound, ""),
        [0xe587] = (ChozoStatueCueKind.PlayFootstepSound, ""),
        [0xe58f] = (ChozoStatueCueKind.Footstep, "V"),
        [0xe5d8] = (ChozoStatueCueKind.Move, "V"),
        [0xe6f0] = (ChozoStatueCueKind.ReleaseSamus, ""),
    };
}
