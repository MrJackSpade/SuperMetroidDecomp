using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteHeadSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations (#641, #2554). Each frame shows a <see cref="EnemyFrameId"/>
/// and each cue is a <see cref="MagdolliteHeadCue"/> with the values it acts on, which only the head carries out.
/// </summary>
internal static class MagdolliteHeadAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteHeadSegment, EnemyFrameId, MagdolliteHeadCue> Segments { get; } = new(
        new Dictionary<MagdolliteHeadSegment, AnimationBeat<EnemyFrameId, MagdolliteHeadCue>[]>
    {
        [P.LeftIdle] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingLeftHigh, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftLow, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 13),
        ],
        [P.LeftSubmerge] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteHeadCue.Signal(K.SinkAndShowParts)),
        ],
        [P.LeftRaisePillar] =
        [
            Cue(new MagdolliteHeadCue.MoveWithPillar(new(0, -1))),
            Show(EnemyFrameId.MagdolliteMoundFacingLeft, 1),
        ],
        [P.LeftPillarRaised] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.FinishSubmerge)),
        ],
        [P.LeftEmerge] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.RestoreOrigins)),
        ],
        [P.LeftLowerPillar] =
        [
            Cue(new MagdolliteHeadCue.MoveWithPillar(new(0, 1))),
            Show(EnemyFrameId.MagdolliteMoundFacingLeft, 1),
        ],
        [P.LeftSurfacing] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteHeadCue.Signal(K.FinishEmerge)),
        ],
        [P.RightIdle] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingRightHigh, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightLow, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 13),
        ],
        [P.RightSubmerge] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingRight, 5),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteHeadCue.Signal(K.SinkAndShowParts)),
        ],
        [P.RightRaisePillar] =
        [
            Cue(new MagdolliteHeadCue.MoveWithPillar(new(0, -1))),
            Show(EnemyFrameId.MagdolliteMoundFacingRight, 1),
        ],
        [P.RightPillarRaised] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.FinishSubmerge)),
        ],
        [P.RightEmerge] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.RestoreOrigins)),
        ],
        [P.RightLowerPillar] =
        [
            Cue(new MagdolliteHeadCue.MoveWithPillar(new(0, 1))),
            Show(EnemyFrameId.MagdolliteMoundFacingRight, 1),
        ],
        [P.RightSurfacing] =
        [
            Cue(new MagdolliteHeadCue.Signal(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingRight, 5),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteHeadCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteHeadCue.Signal(K.FinishEmerge)),
        ],
    });

    /// <summary>The pixels the pillar rises or lowers in a submerge or emerge, one a play of its mound's step.</summary>
    private const int PillarTravel = 24;

    /// <summary>
    /// The animation that starts at each segment the head can start or restart at (#2337): the idle loops, a submerge sinks the head and
    /// then raises the pillar a pixel at a time, an emerge lowers it a pixel at a time and then surfaces the head. A submerge
    /// and an emerge each end on their finishing cue, which moves the head on.
    /// </summary>
    internal static Dictionary<MagdolliteHeadSegment, Animation<MagdolliteHeadSegment>> StartingAt { get; } = new Dictionary<MagdolliteHeadSegment, Animation<MagdolliteHeadSegment>>
    {
        [P.LeftIdle] = Animation<MagdolliteHeadSegment>.Looping([Segments.Whole(P.LeftIdle)]),
        [P.LeftSubmerge] = new([Segments.Whole(P.LeftSubmerge), Segments.Repeated(P.LeftRaisePillar, PillarTravel), Segments.Whole(P.LeftPillarRaised)], AnimationEnd.Holds),
        [P.LeftEmerge] = new([Segments.Whole(P.LeftEmerge), Segments.Repeated(P.LeftLowerPillar, PillarTravel), Segments.Whole(P.LeftSurfacing)], AnimationEnd.Holds),
        [P.RightIdle] = Animation<MagdolliteHeadSegment>.Looping([Segments.Whole(P.RightIdle)]),
        [P.RightSubmerge] = new([Segments.Whole(P.RightSubmerge), Segments.Repeated(P.RightRaisePillar, PillarTravel), Segments.Whole(P.RightPillarRaised)], AnimationEnd.Holds),
        [P.RightEmerge] = new([Segments.Whole(P.RightEmerge), Segments.Repeated(P.RightLowerPillar, PillarTravel), Segments.Whole(P.RightSurfacing)], AnimationEnd.Holds),
    };


    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteHeadCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteHeadCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteHeadCue> Cue(MagdolliteHeadCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteHeadCue>.Event(cue);
}
