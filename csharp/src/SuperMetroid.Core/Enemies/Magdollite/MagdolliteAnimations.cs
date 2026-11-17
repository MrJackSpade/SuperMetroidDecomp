using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MagdolliteCue"/> with the values it acts on.
/// </summary>
internal static class MagdolliteAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteSegment, EnemyFrameId, MagdolliteCue> Segments { get; } = new(
        new Dictionary<MagdolliteSegment, AnimationBeat<EnemyFrameId, MagdolliteCue>[]>
    {
        [P.LeftIdle] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingLeftHigh, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftLow, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 13),
        ],
        [P.LeftThrow] =
        [
            Cue(new MagdolliteCue.PlaySoundIfOnScreen(MagdolliteSoundDefinitions.Throw)),
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Cue(new MagdolliteCue.Signal(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRearingFacingLeft, 26),
            Show(EnemyFrameId.MagdolliteLeaningFacingLeft, 8),
            Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(-8, -4))),
            Show(EnemyFrameId.MagdolliteReachingFacingLeft, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteReachingFacingLeft, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteReachingFacingLeft, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(-8, -4))),
            Show(EnemyFrameId.MagdolliteThrowingFacingLeft, 5),
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.LeftSubmerge] =
        [
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteCue.Signal(K.SinkAndShowParts)),
        ],
        [P.LeftRaisePillar] =
        [
            Cue(new MagdolliteCue.MoveWithPillar(new(0, -1))),
            Show(EnemyFrameId.MagdolliteMoundFacingLeft, 1),
        ],
        [P.LeftPillarRaised] =
        [
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.LeftEmerge] =
        [
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Cue(new MagdolliteCue.Signal(K.RestoreOrigins)),
        ],
        [P.LeftLowerPillar] =
        [
            Cue(new MagdolliteCue.MoveWithPillar(new(0, 1))),
            Show(EnemyFrameId.MagdolliteMoundFacingLeft, 1),
        ],
        [P.LeftSurfacing] =
        [
            Cue(new MagdolliteCue.Signal(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Cue(new MagdolliteCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteSunkFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingLeft, 5),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteHeadFacingLeftMid, 5),
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.RightIdle] =
        [
            Show(EnemyFrameId.MagdolliteHeadFacingRightHigh, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightLow, 13),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 13),
        ],
        [P.RightThrow] =
        [
            Cue(new MagdolliteCue.PlaySoundIfOnScreen(MagdolliteSoundDefinitions.Throw)),
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Cue(new MagdolliteCue.Signal(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRearingFacingRight, 26),
            Show(EnemyFrameId.MagdolliteLeaningFacingRight, 8),
            Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(8, -4))),
            Show(EnemyFrameId.MagdolliteReachingFacingRight, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteReachingFacingRight, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteReachingFacingRight, 5),
            Cue(new MagdolliteCue.Signal(K.SpawnLava)),
            Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(8, -8))),
            Show(EnemyFrameId.MagdolliteThrowingFacingRight, 5),
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.RightSubmerge] =
        [
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingRight, 5),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteCue.Move(new(0, 2))),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteCue.Signal(K.SinkAndShowParts)),
        ],
        [P.RightRaisePillar] =
        [
            Cue(new MagdolliteCue.MoveWithPillar(new(0, -1))),
            Show(EnemyFrameId.MagdolliteMoundFacingRight, 1),
        ],
        [P.RightPillarRaised] =
        [
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.RightEmerge] =
        [
            Cue(new MagdolliteCue.Signal(K.SetAnimationBusy)),
            Cue(new MagdolliteCue.Signal(K.RestoreOrigins)),
        ],
        [P.RightLowerPillar] =
        [
            Cue(new MagdolliteCue.MoveWithPillar(new(0, 1))),
            Show(EnemyFrameId.MagdolliteMoundFacingRight, 1),
        ],
        [P.RightSurfacing] =
        [
            Cue(new MagdolliteCue.Signal(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Cue(new MagdolliteCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteSunkFacingRight, 5),
            Show(EnemyFrameId.MagdolliteSinkingFacingRight, 5),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteCue.Move(new(0, -2))),
            Show(EnemyFrameId.MagdolliteHeadFacingRightMid, 5),
            Cue(new MagdolliteCue.Signal(K.ClearAnimationBusy)),
        ],
        [P.OneSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteOneSegmentPillar),
        ],
        [P.TwoSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteTwoSegmentPillar),
        ],
        [P.ThreeSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteThreeSegmentPillar),
        ],
        [P.FourSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteFourSegmentPillar),
        ],
        [P.FiveSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteFiveSegmentPillar),
        ],
        [P.SixSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteSixSegmentPillar),
        ],
        [P.SevenSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteSevenSegmentPillar),
        ],
        [P.EightSegmentPillar] =
        [
            Hold(EnemyFrameId.MagdolliteEightSegmentPillar),
        ],
        [P.PillarCap] =
        [
            Hold(EnemyFrameId.MagdollitePillarCap),
        ],
    });

    /// <summary>The pixels the pillar rises or lowers in a submerge or emerge, one a play of its mound's step.</summary>
    private const int PillarTravel = 24;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2337): a submerge sinks the head and then
    /// raises the pillar a pixel at a time, an emerge lowers it a pixel at a time and then surfaces the head, and the pillar
    /// stacks and cap are still pictures.
    /// </summary>
    internal static IReadOnlyDictionary<MagdolliteSegment, Animation<MagdolliteSegment>> StartingAt { get; } = new Dictionary<MagdolliteSegment, Animation<MagdolliteSegment>>
    {
        [P.LeftIdle] = new([Whole(P.LeftIdle)], AnimationEnd.LoopsFrom(0)),
        [P.LeftThrow] = new([Whole(P.LeftThrow)], AnimationEnd.Holds),
        [P.LeftSubmerge] = new([Whole(P.LeftSubmerge), Repeated(P.LeftRaisePillar, PillarTravel), Whole(P.LeftPillarRaised)], AnimationEnd.Holds),
        [P.LeftEmerge] = new([Whole(P.LeftEmerge), Repeated(P.LeftLowerPillar, PillarTravel), Whole(P.LeftSurfacing)], AnimationEnd.Holds),
        [P.RightIdle] = new([Whole(P.RightIdle)], AnimationEnd.LoopsFrom(0)),
        [P.RightThrow] = new([Whole(P.RightThrow)], AnimationEnd.Holds),
        [P.RightSubmerge] = new([Whole(P.RightSubmerge), Repeated(P.RightRaisePillar, PillarTravel), Whole(P.RightPillarRaised)], AnimationEnd.Holds),
        [P.RightEmerge] = new([Whole(P.RightEmerge), Repeated(P.RightLowerPillar, PillarTravel), Whole(P.RightSurfacing)], AnimationEnd.Holds),
        [P.OneSegmentPillar] = new([Whole(P.OneSegmentPillar)], AnimationEnd.Holds),
        [P.TwoSegmentPillar] = new([Whole(P.TwoSegmentPillar)], AnimationEnd.Holds),
        [P.ThreeSegmentPillar] = new([Whole(P.ThreeSegmentPillar)], AnimationEnd.Holds),
        [P.FourSegmentPillar] = new([Whole(P.FourSegmentPillar)], AnimationEnd.Holds),
        [P.FiveSegmentPillar] = new([Whole(P.FiveSegmentPillar)], AnimationEnd.Holds),
        [P.SixSegmentPillar] = new([Whole(P.SixSegmentPillar)], AnimationEnd.Holds),
        [P.SevenSegmentPillar] = new([Whole(P.SevenSegmentPillar)], AnimationEnd.Holds),
        [P.EightSegmentPillar] = new([Whole(P.EightSegmentPillar)], AnimationEnd.Holds),
        [P.PillarCap] = new([Whole(P.PillarCap)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteSegment> StartingWith(MagdolliteSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<MagdolliteSegment> Whole(MagdolliteSegment segment) => Repeated(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<MagdolliteSegment> Repeated(MagdolliteSegment segment, int plays) => new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Cue(MagdolliteCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Event(cue);
}
