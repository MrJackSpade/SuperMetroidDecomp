using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<ChozoStatueSegment>[] Entries =
    [
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairInitial, 0xe39d),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairActivated, 0xe3a7),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairActivatedLoop, 0xe3e5),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipInitial, 0xe457),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipActivated, 0xe461),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipActivatedLoop, 0xe49d),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipActivatedLoop2, 0xe4c1),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<ChozoStatueCue>> Instructions = new()
    {
        // Installing a frame routine that returns at once clears the frame behaviour (#1803).
        [0x806b] = new NativeCueShape<ChozoStatueCue>("a frame behaviour cue",
            cue => cue is ChozoStatueCue.SetFrameBehavior or ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.ClearFrameBehavior }, "E"),
        [0x8074] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.ClearFrameBehavior)),
        [0xe429] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.StartLoweringAcid)),
        [0xe436] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.SetLoweredAcidPosition)),
        [0xe43d] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.UnlockSamus)),
        [0xe57f] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.PlayGrabSound)),
        [0xe587] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.PlayFootstepSound)),
        [0xe58f] = NativeCueShape<ChozoStatueCue>.Of<ChozoStatueCue.Footstep>("F"),
        [0xe5d8] = NativeCueShape<ChozoStatueCue>.Of<ChozoStatueCue.Move>("R"),
        [0xe6f0] = NativeCueShape<ChozoStatueCue>.Exactly(new ChozoStatueCue.Signal(ChozoStatueCueKind.ReleaseSamus)),
    };
}
