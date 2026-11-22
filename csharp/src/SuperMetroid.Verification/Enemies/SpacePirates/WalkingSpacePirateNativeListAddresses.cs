using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// The native bank-$B2 lists the Walking Space Pirate ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class WalkingSpacePirateNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<WalkingSpacePirateSegment>[] Entries =
    [
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.FlinchFacingLeft, 0xfb4c),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.FlinchFacingRight, 0xfb58),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.WalkingLeft, 0xfb64),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.WalkingLeftStride, 0xfb68),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.FireLasersLeft, 0xfb8c),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.LookingFacingLeft, 0xfbc6),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.WalkingRight, 0xfbe6),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.WalkingRightStride, 0xfbea),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.FireLasersRight, 0xfc0e),
        new NativeProgramEntry<WalkingSpacePirateSegment>(WalkingSpacePirateSegment.LookingFacingRight, 0xfc48),
    ];

    /// <summary>The strides the walks only run on into, after their heading cues (#2276).</summary>
    internal static readonly IReadOnlySet<WalkingSpacePirateSegment> OnlyRunOnInto = new HashSet<WalkingSpacePirateSegment>
    {
        WalkingSpacePirateSegment.WalkingLeftStride,
        WalkingSpacePirateSegment.WalkingRightStride,
    };

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<WalkingSpacePirateCue>> Instructions = new()
    {
        [0xfc68] = NativeCueShape<WalkingSpacePirateCue>.Of<WalkingSpacePirateCue.FireLaser>("V", "a laser to the left", laser => laser.Direction == HorizontalDirection.Left),
        [0xfc90] = NativeCueShape<WalkingSpacePirateCue>.Of<WalkingSpacePirateCue.FireLaser>("V", "a laser to the right", laser => laser.Direction == HorizontalDirection.Right),
        // The routine setter begins an action, a walk with its heading (#2165).
        [0xfcb8] = new NativeCueShape<WalkingSpacePirateCue>(nameof(WalkingSpacePirateCue.Begin),
            cue => cue is WalkingSpacePirateCue.Begin or WalkingSpacePirateCue.BeginHeading, "E"),
        [0xfcc8] = NativeCueShape<WalkingSpacePirateCue>.Exactly(new WalkingSpacePirateCue.Signal()),
    };
}
