using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCue>;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WalkingSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>The updates each of the walking pirate's striding poses shows for.</summary>
    private const int StridePoseUpdates = 10;

    /// <summary>The walking pirate's segments facing left.</summary>
    private static FacedSegments LeftSegments { get; } = new(P.FlinchFacingLeft, P.WalkingLeft, P.WalkingLeftStride, P.FireLasersLeft, P.LookingFacingLeft);

    /// <summary>The walking pirate's segments facing right.</summary>
    private static FacedSegments RightSegments { get; } = new(P.FlinchFacingRight, P.WalkingRight, P.WalkingRightStride, P.FireLasersRight, P.LookingFacingRight);

    /// <summary>Every segment's beats, each written once over a side (#2557).</summary>
    internal static AnimationSegments<WalkingSpacePirateSegment, EnemyFrameId, WalkingSpacePirateCue> Segments { get; } = new(
        FacedBeats(WalkingSpacePirateSide.Left, LeftSegments).Concat(FacedBeats(WalkingSpacePirateSide.Right, RightSegments)).ToDictionary());

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: a flinch walks on the way the pirate faces,
    /// and its lasers and its look turn it to walk the other way.
    /// </summary>
    internal static Dictionary<WalkingSpacePirateSegment, Animation<WalkingSpacePirateSegment>> StartingAt { get; } =
        FacedStarts(LeftSegments, RightSegments).Concat(FacedStarts(RightSegments, LeftSegments)).ToDictionary();

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WalkingSpacePirateSegment> StartingWith(WalkingSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>The segments a walking pirate plays facing one way.</summary>
    private sealed record FacedSegments(P Flinch, P Walking, P WalkingStride, P FireLasers, P Looking);

    /// <summary>The beats of <paramref name="segments"/>, the pirate facing as <paramref name="side"/> does.</summary>
    private static Dictionary<P, AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>[]> FacedBeats(WalkingSpacePirateSide side, FacedSegments segments) => new()
    {
        [segments.Flinch] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(side.Flinch, 16),
        ],
        [segments.Walking] =
        [
            Cue(new WalkingSpacePirateCue.BeginHeading(WalkingSpacePirateAction.Walking, side.Facing)),
        ],
        [segments.WalkingStride] = Beats.Steady(StridePoseUpdates, side.Striding),
        [segments.FireLasers] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(side.Standing, 24),
            Show(side.ClawsLifting, 8),
            Show(side.ClawsForward, 8),
            Show(side.ClawsRaised, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(side.Facing, LaserHeight: 8)),
            Show(side.ClawsRaisedLeaning, 8),
            Cue(new WalkingSpacePirateCue.FireLaser(side.Facing, LaserHeight: 2)),
            Show(side.Firing, 24),
            Cue(new WalkingSpacePirateCue.FireLaser(side.Facing, LaserHeight: -8)),
            Show(side.ClawsRaisedLeaning, 8),
            Show(side.ClawsRaised, 8),
            Show(side.ClawsForward, 8),
            Show(side.ClawsLifting, 8),
            Cue(new WalkingSpacePirateCue.Signal(K.ChooseMovement)),
        ],
        [segments.Looking] =
        [
            Cue(new WalkingSpacePirateCue.Begin(WalkingSpacePirateAction.StandingStill)),
            Show(side.LookingAtViewer, 32),
            Show(side.Standing, 10),
            Show(side.LookingAway, 32),
            Show(side.Standing, 10),
            Show(side.LookingAtViewer, 32),
            Show(EnemyFrameId.WalkingPirateFacingViewer, 8),
        ],
    };

    /// <summary>
    /// The animations a pirate facing one way starts at its <paramref name="segments"/>: it walks on its own way after a
    /// flinch, and the <paramref name="opposite"/> way after its lasers or its look.
    /// </summary>
    private static Dictionary<P, Animation<P>> FacedStarts(FacedSegments segments, FacedSegments opposite) => new()
    {
        [segments.Flinch] = Walk([segments.Flinch], segments.Walking, segments.WalkingStride),
        [segments.Walking] = Walk([], segments.Walking, segments.WalkingStride),
        [segments.FireLasers] = Walk([segments.FireLasers, segments.Looking], opposite.Walking, opposite.WalkingStride),
        [segments.Looking] = Walk([segments.Looking], opposite.Walking, opposite.WalkingStride),
    };

    /// <summary>
    /// Plays each of <paramref name="before"/> whole, then a walk (#2276, #2414): its heading cue <paramref name="heading"/> once
    /// and its stride <paramref name="stride"/> for good.
    /// </summary>
    private static Animation<P> Walk(IReadOnlyList<P> before, P heading, P stride) =>
        Animation<P>.LeadingInto([.. before.Select(Segments.Whole), Segments.Whole(heading)], [Segments.Whole(stride)]);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WalkingSpacePirateCue>.Event(cue);
}
