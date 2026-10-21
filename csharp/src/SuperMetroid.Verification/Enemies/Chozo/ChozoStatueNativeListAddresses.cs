using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>
/// The native bank-$AA lists the Chozo statue ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (E pre-instruction, F footstep offset,
/// R carry record).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ChozoStatueNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xaa;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ChozoStatueProgram>[] Entries =
    [
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.LowerNorfairInitial, 0xe39d),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.LowerNorfairActivated, 0xe3a7),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.LowerNorfairActivatedLoop, 0xe3e5),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.WreckedShipInitial, 0xe457),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.WreckedShipActivated, 0xe461),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.WreckedShipActivatedLoop, 0xe49d),
        new NativeProgramEntry<ChozoStatueProgram>(ChozoStatueProgram.WreckedShipActivatedLoop2, 0xe4c1),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<ChozoStatueCue>> Instructions = new()
    {
        [0x806b] = NativeCueShape<ChozoStatueCue>.Of<ChozoStatueCue.SetPreInstruction>("E"),
        [0x8074] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.ClearPreInstruction)),
        [0xe429] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.StartLoweringAcid)),
        [0xe436] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.SetLoweredAcidPosition)),
        [0xe43d] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.UnlockSamus)),
        [0xe57f] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.PlayGrabSound)),
        [0xe587] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.PlayFootstepSound)),
        [0xe58f] = NativeCueShape<ChozoStatueCue>.Of<ChozoStatueCue.Footstep>("F"),
        [0xe5d8] = NativeCueShape<ChozoStatueCue>.Of<ChozoStatueCue.Move>("R"),
        [0xe6f0] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Command(ChozoStatueCueKind.ReleaseSamus)),
    };
}
