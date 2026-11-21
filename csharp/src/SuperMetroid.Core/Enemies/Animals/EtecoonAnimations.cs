using P = SuperMetroid.Core.Enemies.Animals.EtecoonSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class EtecoonAnimations
{
    /// <summary>
    /// A hop's arm poses in the order it shows them, facing as its poses do: <paramref name="raising"/>, <paramref name="up"/>,
    /// <paramref name="flung"/>, then back through <paramref name="up"/> and <paramref name="raising"/>.
    /// </summary>
    private static EnemyFrameId[] HopArms(EnemyFrameId raising, EnemyFrameId up, EnemyFrameId flung) => [raising, up, flung, up, raising];

    /// <summary>How long each of a hop's arm poses shows.</summary>
    private static readonly int[] HopPace = [12, 12, 6, 12, 12];

    /// <summary>A hop's arm poses facing right, which the flex also plays at its own pace.</summary>
    private static readonly EnemyFrameId[] HopArmsFacingRight =
        HopArms(EnemyFrameId.EtecoonHopArmsRaisingFacingRight, EnemyFrameId.EtecoonHopArmsUpFacingRight, EnemyFrameId.EtecoonHopArmsFlungFacingRight);

    /// <summary>A jump's spin over a facing's four quarter turns, <paramref name="turned0"/> to <paramref name="turned270"/>, three updates each.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Spin(EnemyFrameId turned0, EnemyFrameId turned90, EnemyFrameId turned180, EnemyFrameId turned270) =>
        [.. ((EnemyFrameId[])[turned0, turned90, turned180, turned270]).Select(pose => Show(pose, 3))];

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EtecoonSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<EtecoonSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.LookRightAtSamusAndRunLeft] =
        [
            Show(EnemyFrameId.EtecoonLookingRight, 5),
        ],
        [P.BeginRunningLeft] =
        [
            Show(EnemyFrameId.EtecoonTurningLeft, 1),
        ],
        [P.RunningLeft] = Stride(EtecoonStride.Left, 5),
        [P.WallJumpLeft] =
        [
            Show(EnemyFrameId.EtecoonLeapingLeft, 8),
        ],
        [P.JumpingLeft] = Spin(EnemyFrameId.EtecoonSpinningLeftTurned0, EnemyFrameId.EtecoonSpinningLeftTurned90,
            EnemyFrameId.EtecoonSpinningLeftTurned180, EnemyFrameId.EtecoonSpinningLeftTurned270),
        [P.HoppingFacingLeft] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingLeft)],
        [P.ContinueHoppingFacingLeft] = [.. HopArms(EnemyFrameId.EtecoonHopArmsRaisingFacingLeft, EnemyFrameId.EtecoonHopArmsUpFacingLeft,
            EnemyFrameId.EtecoonHopArmsFlungFacingLeft).Zip(HopPace, Show)],
        [P.WallJumpLeftEligible] = [Hold(EnemyFrameId.EtecoonClingingFacingLeft)],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(EnemyFrameId.EtecoonLookingLeft, 5),
        ],
        [P.BeginRunningRight] =
        [
            Show(EnemyFrameId.EtecoonTurningRight, 1),
        ],
        [P.RunningRight] = Stride(EtecoonStride.Right, 5),
        [P.WallJumpRight] =
        [
            Show(EnemyFrameId.EtecoonLeapingRight, 8),
        ],
        [P.JumpingRight] = Spin(EnemyFrameId.EtecoonSpinningRightTurned0, EnemyFrameId.EtecoonSpinningRightTurned90,
            EnemyFrameId.EtecoonSpinningRightTurned180, EnemyFrameId.EtecoonSpinningRightTurned270),
        [P.HoppingFacingRight] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingRight)],
        [P.ContinueHoppingFacingRight] = [.. HopArmsFacingRight.Zip(HopPace, Show)],
        [P.WallJumpRightEligible] = [Hold(EnemyFrameId.EtecoonClingingFacingRight)],
        [P.StartingCrouch] = [Hold(EnemyFrameId.EtecoonHopCrouchFacingRight)],
        [P.Flexing] = [.. ((EnemyFrameId[])[EnemyFrameId.EtecoonHopCrouchFacingRight, .. HopArmsFacingRight]).Select(pose => Show(pose, FlexPoseUpdates))],
        [P.FlexEnd] =
        [
            Show(EnemyFrameId.EtecoonFacingViewer, 32),
            Show(EnemyFrameId.EtecoonLookingLeft, 32),
        ],
    });

    /// <summary>How long each pose of a woken Etecoon's flex shows.</summary>
    private const int FlexPoseUpdates = 8;

    /// <summary>How many times a woken Etecoon's flex plays its six hop poses.</summary>
    private const int FlexRuns = 4;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): a crouch, a cling and the
    /// placed Etecoon's crouch are still pictures, and a wall jump either way leaps and then spins for good.
    /// </summary>
    private static Dictionary<EtecoonSegment, Animation<EtecoonSegment>> StartingAt { get; } = new Dictionary<EtecoonSegment, Animation<EtecoonSegment>>
    {
        [P.LookRightAtSamusAndRunLeft] = new([Segments.Whole(P.LookRightAtSamusAndRunLeft)], AnimationEnd.Holds),
        [P.BeginRunningLeft] = Animation<EtecoonSegment>.LeadingInto([Segments.Whole(P.BeginRunningLeft)], [Segments.Whole(P.RunningLeft)]),
        [P.RunningLeft] = Animation<EtecoonSegment>.Looping([Segments.Whole(P.RunningLeft)]),
        [P.WallJumpLeft] = Animation<EtecoonSegment>.LeadingInto([Segments.Whole(P.WallJumpLeft)], [Segments.Whole(P.JumpingLeft)]),
        [P.HoppingFacingLeft] = new([Segments.Whole(P.HoppingFacingLeft)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingLeft] = new([Segments.Whole(P.ContinueHoppingFacingLeft)], AnimationEnd.Holds),
        [P.WallJumpLeftEligible] = new([Segments.Whole(P.WallJumpLeftEligible)], AnimationEnd.Holds),
        [P.LookLeftAtSamusAndRunRight] = new([Segments.Whole(P.LookLeftAtSamusAndRunRight)], AnimationEnd.Holds),
        [P.BeginRunningRight] = Animation<EtecoonSegment>.LeadingInto([Segments.Whole(P.BeginRunningRight)], [Segments.Whole(P.RunningRight)]),
        [P.RunningRight] = Animation<EtecoonSegment>.Looping([Segments.Whole(P.RunningRight)]),
        [P.WallJumpRight] = Animation<EtecoonSegment>.LeadingInto([Segments.Whole(P.WallJumpRight)], [Segments.Whole(P.JumpingRight)]),
        [P.JumpingRight] = Animation<EtecoonSegment>.Looping([Segments.Whole(P.JumpingRight)]),
        [P.HoppingFacingRight] = new([Segments.Whole(P.HoppingFacingRight)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingRight] = new([Segments.Whole(P.ContinueHoppingFacingRight)], AnimationEnd.Holds),
        [P.WallJumpRightEligible] = new([Segments.Whole(P.WallJumpRightEligible)], AnimationEnd.Holds),
        [P.StartingCrouch] = new([Segments.Whole(P.StartingCrouch)], AnimationEnd.Holds),
    };

    /// <summary>The flex its wake starts (#2061): its hop poses played over (<see cref="FlexRuns"/>), then its closing poses, held.</summary>
    internal static Animation<EtecoonSegment> Flexing { get; } = new([Segments.Repeated(P.Flexing, FlexRuns), Segments.Whole(P.FlexEnd)], AnimationEnd.Holds);

    /// <summary>
    /// The left hop entered at its crest (#2061), as hitting a ceiling enters it: the crest, then the way down.
    /// </summary>
    internal static AnimationPlayback<EtecoonSegment> LeftHopFromCrest { get; } = new(StartingAt[P.ContinueHoppingFacingLeft],
        Segments.PlaceOfFrame(StartingAt[P.ContinueHoppingFacingLeft], P.ContinueHoppingFacingLeft, EnemyFrameId.EtecoonHopArmsFlungFacingLeft));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EtecoonSegment> StartingWith(EtecoonSegment segment) => StartingAt[segment];

    /// <summary>One <paramref name="stride"/> (#2477), each of its poses shown for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Stride(EtecoonStride stride, int updates) =>
        [.. stride.Poses.Select(pose => Show(pose, updates))];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}
