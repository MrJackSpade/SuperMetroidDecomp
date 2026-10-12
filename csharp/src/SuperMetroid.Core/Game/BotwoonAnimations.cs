using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The eight directions Botwoon's head and body segments face, clockwise from up: the order of
/// the cartridge's 32-unit angle octants, so octant <c>n</c> faces <c>(BotwoonFacing)n</c>.
/// </summary>
internal enum BotwoonFacing
{
    /// <summary>Octant 0.</summary>
    Up,
    /// <summary>Octant 1.</summary>
    UpRight,
    /// <summary>Octant 2.</summary>
    Right,
    /// <summary>Octant 3.</summary>
    DownRight,
    /// <summary>Octant 4.</summary>
    Down,
    /// <summary>Octant 5.</summary>
    DownLeft,
    /// <summary>Octant 6.</summary>
    Left,
    /// <summary>Octant 7.</summary>
    UpLeft,
}

/// <summary>What Botwoon's head animation is doing.</summary>
internal enum BotwoonHeadProgramKind
{
    /// <summary>Travelling inside the wall: nothing drawn (<c>InstList_Botwoon_Hide</c>).</summary>
    Hidden,
    /// <summary>Travelling outside: the closed mouth faces the movement (<c>InstList_Botwoon_MouthClosed_*</c>).</summary>
    Moving,
    /// <summary>Aiming at Samus, then opening to spit (<c>InstList_Botwoon_Spit_*</c>).</summary>
    Spitting,
}

/// <summary>Botwoon's head animation: what it does and which way it faces.</summary>
internal readonly record struct BotwoonHeadProgram(BotwoonHeadProgramKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden head. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonHeadProgram Hidden => new(BotwoonHeadProgramKind.Hidden, BotwoonFacing.Up);

    /// <summary>
    /// The visible moving head for a cartridge byte angle: its octant selects one of the eight
    /// closed-mouth programs (<c>InstListPointers_Botwoon</c>, $B3:946B).
    /// </summary>
    internal static BotwoonHeadProgram MovingToward(byte angle) =>
        new(BotwoonHeadProgramKind.Moving, BotwoonAnimations.FacingOfOctant(angle >> 5));

    /// <summary>
    /// The spitting head for a cartridge byte angle, biased half an octant so Samus is aimed at the
    /// nearest octant (<c>InstListPointers_Botwoon_spit</c>, $B3:948B).
    /// </summary>
    internal static BotwoonHeadProgram SpittingToward(byte angle) =>
        new(BotwoonHeadProgramKind.Spitting, BotwoonAnimations.FacingOfOctant(unchecked((byte)(angle + 16)) >> 5));
}

/// <summary>A drawn head pose; <see cref="Visible"/> false is the empty frame of the hidden head.</summary>
internal readonly record struct BotwoonHeadFrame(bool Visible, BotwoonFacing Facing, bool MouthOpen)
{
    /// <summary>The empty frame.</summary>
    internal static BotwoonHeadFrame Hidden => new(false, BotwoonFacing.Up, false);

    /// <summary>The closed mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Closed(BotwoonFacing facing) => new(true, facing, false);

    /// <summary>The open mouth facing <paramref name="facing"/>.</summary>
    internal static BotwoonHeadFrame Open(BotwoonFacing facing) => new(true, facing, true);
}

/// <summary>Gameplay cues published by the head animation.</summary>
internal enum BotwoonHeadCue
{
    /// <summary>Sets the head's collision radius for its facing (<c>Instruction_Botwoon_EnemyRadius_*</c>).</summary>
    ApplyFacingRadius,
    /// <summary>Queues the spit sound (<c>Instruction_Botwoon_QueueSpitSFX</c>).</summary>
    QueueSpitSound,
    /// <summary>Marks the frame the spit volley leaves the mouth (<c>Instruction_Botwoon_SetSpittingFlag</c>).</summary>
    ReachSpitFrame,
}

/// <summary>What a body-segment actor shows.</summary>
internal enum BotwoonSegmentProgramKind
{
    /// <summary>An outside body link: four frames that cycle (<c>InstList_EnemyProjectile_BotwoonsBody_*</c>).</summary>
    Body,
    /// <summary>The outside tail cap: one frame (<c>InstList_EnemyProjectile_BotwoonsTail_*</c>).</summary>
    Tail,
    /// <summary>A link or tail inside the wall: nothing drawn (<c>InstList_EnemyProjectile_BotwoonsBodyTail_Hidden</c>).</summary>
    Hidden,
}

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonSegmentProgram(BotwoonSegmentProgramKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Hidden => new(BotwoonSegmentProgramKind.Hidden, BotwoonFacing.Up);

    /// <summary>
    /// Decodes the selector the AI keeps in the segment's direction word: an even byte offset 0 through
    /// 62 into <c>BotwoonsBodyTail_InstListPointers</c> ($86:E9F1). Word index bit 4 selects the tail,
    /// bit 3 the hidden half of a region, and bits 0-2 the facing octant.
    /// </summary>
    internal static BotwoonSegmentProgram FromSelector(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset >= 64)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset, "Botwoon segment selector must be even and zero through 62.");
        }
        int entry = byteOffset >> 1;
        if ((entry & 8) != 0)
            return Hidden;
        return new((entry & 16) != 0 ? BotwoonSegmentProgramKind.Tail : BotwoonSegmentProgramKind.Body,
            BotwoonAnimations.FacingOfOctant(entry & 7));
    }
}

/// <summary>What a Botwoon projectile frame depicts.</summary>
internal enum BotwoonProjectileFrameKind
{
    /// <summary>One of a body link's four cycling frames.</summary>
    Body,
    /// <summary>The tail cap.</summary>
    Tail,
    /// <summary>The empty frame of a hidden segment.</summary>
    Hidden,
    /// <summary>One of the spit's five cycling frames.</summary>
    Spit,
}

/// <summary>A drawn Botwoon projectile frame: kind, facing (body and tail) and phase (body and spit).</summary>
internal readonly record struct BotwoonProjectileFrame(BotwoonProjectileFrameKind Kind, BotwoonFacing Facing, int Phase);

/// <summary>The typed animations that replace Botwoon's native instruction lists (#641).</summary>
internal static class BotwoonAnimations
{
    /// <summary>Number of frames in a body link's cycle.</summary>
    internal const int BodyPhaseCount = 4;

    /// <summary>Number of frames in the spit's cycle.</summary>
    internal const int SpitPhaseCount = 5;

    private static readonly BotwoonFacing[] Octants =
    [
        BotwoonFacing.Up, BotwoonFacing.UpRight, BotwoonFacing.Right, BotwoonFacing.DownRight,
        BotwoonFacing.Down, BotwoonFacing.DownLeft, BotwoonFacing.Left, BotwoonFacing.UpLeft,
    ];

    private const int MouthClosedBeforeSpit = 32;
    private const int MouthOpenAfterSpit = 16;
    private const int MouthOpenAfterSpitLeft = 25;
    private const int BodyFrameDuration = 8;
    private const int SpitFrameDuration = 3;

    private static readonly AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> HiddenHead =
        new(AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Hidden, 1),
            AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    private static readonly Dictionary<BotwoonHeadProgram, AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue>> HeadTimelines =
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            (new BotwoonHeadProgram(BotwoonHeadProgramKind.Moving, facing), MovingHead(facing)),
            (new BotwoonHeadProgram(BotwoonHeadProgramKind.Spitting, facing), SpittingHead(facing)),
        }).Append((BotwoonHeadProgram.Hidden, HiddenHead)).ToDictionary(entry => entry.Item1, entry => entry.Item2);

    private static readonly Dictionary<BotwoonSegmentProgram, AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue>> SegmentTimelines =
        Enum.GetValues<BotwoonFacing>().SelectMany(facing => new[]
        {
            (new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Body, facing), BodyLink(facing)),
            (new BotwoonSegmentProgram(BotwoonSegmentProgramKind.Tail, facing), StillSegment(new(BotwoonProjectileFrameKind.Tail, facing, 0))),
        }).Append((BotwoonSegmentProgram.Hidden, StillSegment(new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0))))
            .ToDictionary(entry => entry.Item1, entry => entry.Item2);

    /// <summary>The spit's cycling animation (<c>InstList_EnemyProjectile_BotwoonsSpit</c>).</summary>
    internal static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> Spit { get; } = new(
        [.. Enumerable.Range(0, SpitPhaseCount).Select(phase => AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(
            new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase), SpitFrameDuration)),
         AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Loop]);

    /// <summary>The facing of cartridge angle octant <paramref name="octant"/>, zero (up) through seven, clockwise.</summary>
    internal static BotwoonFacing FacingOfOctant(int octant) =>
        (uint)octant < (uint)Octants.Length
            ? Octants[octant]
            : throw new ArgumentOutOfRangeException(nameof(octant), octant, "A cartridge angle octant is zero through seven.");

    /// <summary>The head timeline for <paramref name="program"/>.</summary>
    internal static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> Head(BotwoonHeadProgram program) =>
        HeadTimelines.TryGetValue(program, out var timeline)
            ? timeline
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Botwoon has no such head program.");

    /// <summary>The body-segment timeline for <paramref name="program"/>.</summary>
    internal static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> Segment(BotwoonSegmentProgram program) =>
        SegmentTimelines.TryGetValue(program, out var timeline)
            ? timeline
            : throw new ArgumentOutOfRangeException(nameof(program), program, "Botwoon has no such segment program.");

    /// <summary>
    /// The head's collision half-extents facing <paramref name="facing"/>: tall vertically, wide
    /// horizontally and square on the diagonals.
    /// </summary>
    internal static Extent16<RoomPixels> HeadRadius(BotwoonFacing facing) => facing switch
    {
        BotwoonFacing.Up or BotwoonFacing.Down => new(8, 16),
        BotwoonFacing.Left or BotwoonFacing.Right => new(16, 8),
        BotwoonFacing.UpRight or BotwoonFacing.DownRight or BotwoonFacing.DownLeft or BotwoonFacing.UpLeft => new(12, 12),
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };

    private static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> MovingHead(BotwoonFacing facing) => new(
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ApplyFacingRadius),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Closed(facing), 1),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    private static AnimationTimeline<BotwoonHeadFrame, BotwoonHeadCue> SpittingHead(BotwoonFacing facing) => new(
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Closed(facing), MouthClosedBeforeSpit),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ApplyFacingRadius),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.QueueSpitSound),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Publish(BotwoonHeadCue.ReachSpitFrame),
        // The cartridge holds the open mouth longer only when spitting left ($B3:93B9 is $19, the rest $10).
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Show(BotwoonHeadFrame.Open(facing),
            facing == BotwoonFacing.Left ? MouthOpenAfterSpitLeft : MouthOpenAfterSpit),
        AnimationStep<BotwoonHeadFrame, BotwoonHeadCue>.Hold);

    private static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> BodyLink(BotwoonFacing facing) => new(
        [.. Enumerable.Range(0, BodyPhaseCount).Select(phase => AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(
            new(BotwoonProjectileFrameKind.Body, facing, phase), BodyFrameDuration)),
         AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Loop]);

    private static AnimationTimeline<BotwoonProjectileFrame, NoAnimationCue> StillSegment(BotwoonProjectileFrame frame) => new(
        AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Show(frame, 1),
        AnimationStep<BotwoonProjectileFrame, NoAnimationCue>.Hold);
}
