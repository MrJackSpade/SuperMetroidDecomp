using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteBodySegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite rising body's pillar stacks (#641, #2554). Each frame shows a <see cref="EnemyFrameId"/> and no beat raises a
/// cue for its owner.
/// </summary>
internal static class MagdolliteBodyAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteBodySegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MagdolliteBodySegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
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
    });

    /// <summary>The animation that starts at each pillar stack (#2337): each is a still picture.</summary>
    internal static Dictionary<MagdolliteBodySegment, Animation<MagdolliteBodySegment>> StartingAt { get; } = new Dictionary<MagdolliteBodySegment, Animation<MagdolliteBodySegment>>
    {
        [P.OneSegmentPillar] = new([Segments.Whole(P.OneSegmentPillar)], AnimationEnd.Holds),
        [P.TwoSegmentPillar] = new([Segments.Whole(P.TwoSegmentPillar)], AnimationEnd.Holds),
        [P.ThreeSegmentPillar] = new([Segments.Whole(P.ThreeSegmentPillar)], AnimationEnd.Holds),
        [P.FourSegmentPillar] = new([Segments.Whole(P.FourSegmentPillar)], AnimationEnd.Holds),
        [P.FiveSegmentPillar] = new([Segments.Whole(P.FiveSegmentPillar)], AnimationEnd.Holds),
        [P.SixSegmentPillar] = new([Segments.Whole(P.SixSegmentPillar)], AnimationEnd.Holds),
        [P.SevenSegmentPillar] = new([Segments.Whole(P.SevenSegmentPillar)], AnimationEnd.Holds),
        [P.EightSegmentPillar] = new([Segments.Whole(P.EightSegmentPillar)], AnimationEnd.Holds),
    };


    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}
