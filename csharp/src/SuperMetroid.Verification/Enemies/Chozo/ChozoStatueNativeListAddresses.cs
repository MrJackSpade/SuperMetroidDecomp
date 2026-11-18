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
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairIdle, 0xe39d),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairActivated, 0xe3a7),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairFlexing, 0xe3e5),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.LowerNorfairRelease, 0xe401),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipIdle, 0xe457),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipActivated, 0xe461),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipFlexing, 0xe49d),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipWalkingWithSamus, 0xe4c1),
        new NativeProgramEntry<ChozoStatueSegment>(ChozoStatueSegment.WreckedShipSettingSamusDown, 0xe529),
    ];

    /// <summary>The native routine that installs the frame routine that returns at once, which only the activation runs (#2476).</summary>
    internal const ushort ClearsFrameRoutine = 0x8074;

    /// <summary>
    /// Whether the native instruction <paramref name="word"/> with <paramref name="operands"/> is one the typed animations
    /// omit (#2476): installing a frame routine that returns at once, which only the activation does.
    /// </summary>
    internal static bool Omitted(ushort word, IReadOnlyList<ushort> operands) =>
        word == ClearsFrameRoutine || word == 0x806b && ChozoNativeWords.ActivatedFrameRoutineWords.Contains(operands[0]);

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<ChozoStatueCue>> Instructions = new()
    {
        // Installing the hand wait starts awaiting Samus (#1803). Installing a routine that returns at once, which only the
        // activation does, is omitted (Omitted): the statue's activation phase says it from the update that starts it (#2476).
        [0x806b] = new NativeCueShape<ChozoStatueCue>("an awaiting-hand cue",
            cue => cue is ChozoStatueCue.Signal { Kind: ChozoStatueCueKind.AwaitHand }, "E"),
        [ClearsFrameRoutine] = new NativeCueShape<ChozoStatueCue>("no cue (omitted)", _ => false, ""),
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
