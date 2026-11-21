using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// The native bank-$A8 lists the Magdollite ROM oracle (#641) walks: each part's programs' entry points and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs, one set for each of the head, the body and the overlay.
/// </summary>
internal static class MagdolliteNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each of the head's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MagdolliteHeadSegment>[] HeadEntries =
    [
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftIdle, 0xac9c),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftSubmerge, 0xacde),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftRaisePillar, 0xacfe),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftPillarRaised, 0xad08),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftEmerge, 0xad0c),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftLowerPillar, 0xad14),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.LeftSurfacing, 0xad1e),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightIdle, 0xad3c),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightSubmerge, 0xad7e),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightRaisePillar, 0xad9e),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightPillarRaised, 0xada8),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightEmerge, 0xadac),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightLowerPillar, 0xadb4),
        new NativeProgramEntry<MagdolliteHeadSegment>(MagdolliteHeadSegment.RightSurfacing, 0xadbe),
    ];

    /// <summary>Each of the body's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MagdolliteBodySegment>[] BodyEntries =
    [
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.OneSegmentPillar, 0xaddc),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.TwoSegmentPillar, 0xade2),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.ThreeSegmentPillar, 0xade8),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.FourSegmentPillar, 0xadee),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.FiveSegmentPillar, 0xadf4),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.SixSegmentPillar, 0xadfa),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.SevenSegmentPillar, 0xae00),
        new NativeProgramEntry<MagdolliteBodySegment>(MagdolliteBodySegment.EightSegmentPillar, 0xae06),
    ];

    /// <summary>Each of the overlay's typed programs' native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MagdolliteOverlaySegment>[] OverlayEntries =
    [
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.LeftThrow, 0xacb0),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.LeftLavaFling, 0xacc2),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.LeftThrowFinish, 0xacd4),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.RightThrow, 0xad50),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.RightLavaFling, 0xad62),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.RightThrowFinish, 0xad74),
        new NativeProgramEntry<MagdolliteOverlaySegment>(MagdolliteOverlaySegment.PillarCap, 0xae0c),
    ];

    /// <summary>
    /// The native routine that ends a part's wait for its animation (#2439): the finishing cue of the submerge, the emergence
    /// or the throw whose list ends with it, which its entry in the instructions therefore names no single cue for.
    /// </summary>
    internal const ushort EndsPartWait = 0xae45;

    /// <summary>Each owner instruction the head's lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MagdolliteNativeListAddressesInstruction<MagdolliteHeadCue>> HeadInstructions = new()
    {
        [0xae26] = new(new MagdolliteHeadCue.Move(new(0, 2)), ""),
        [0xae30] = new(new MagdolliteHeadCue.Move(new(0, -2)), ""),
        [0xae3a] = new(null, ""),
        [EndsPartWait] = new(null, ""),
        [0xae50] = new(new MagdolliteHeadCue.MoveWithPillar(new(0, -1)), ""),
        [0xae5a] = new(new MagdolliteHeadCue.MoveWithPillar(new(0, 1)), ""),
        [0xae64] = new(new MagdolliteHeadCue.Signal(MagdolliteHeadCueKind.SinkAndShowParts), ""),
        [0xae88] = new(new MagdolliteHeadCue.Signal(MagdolliteHeadCueKind.RestoreOrigins), ""),
        [0xae96] = new(new MagdolliteHeadCue.Signal(MagdolliteHeadCueKind.SurfaceAndHideParts), ""),
    };

    /// <summary>Each owner instruction the overlay's lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, MagdolliteNativeListAddressesInstruction<MagdolliteOverlayCue>> OverlayInstructions = new()
    {
        [0xae12] = new(null, "V"),
        [0xae3a] = new(null, ""),
        [EndsPartWait] = new(null, ""),
        [0xaeba] = new(new MagdolliteOverlayCue.Signal(MagdolliteOverlayCueKind.SpawnLava), ""),
        [0xaeca] = new(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(8, -4)), ""),
        [0xaee4] = new(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(-8, -4)), ""),
        [0xaefe] = new(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(8, -8)), ""),
        [0xaf18] = new(new MagdolliteOverlayCue.ShiftFromThrowOrigin(new(-8, -4)), ""),
        [0xaf44] = new(new MagdolliteOverlayCue.Signal(MagdolliteOverlayCueKind.StartCooldown), ""),
    };

    /// <summary>
    /// The native routine a throw, a submerge and an emerge call as they start (#2439): it marked the part waiting for its
    /// animation, which the part's own phase already says from the update that starts the animation, so the animations have
    /// no event for it.
    /// </summary>
    internal static readonly HashSet<ushort> OmittedInstructions = [0xae3a];
}
