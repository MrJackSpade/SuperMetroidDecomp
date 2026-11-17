using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Dragon.DragonCueKind;
using P = SuperMetroid.Core.Enemies.Dragon.DragonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="DragonCue"/> naming what it does.
/// </summary>
internal static class DragonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DragonSegment, EnemyFrameId, DragonCue> Segments { get; } = new(
        new Dictionary<DragonSegment, AnimationBeat<EnemyFrameId, DragonCue>[]>
    {
        [P.IdleFacingLeft] = [Hold(EnemyFrameId.DragonBodyFacingLeft)],
        [P.WingsFacingLeft] =
        [
            Show(EnemyFrameId.DragonWingFacingLeftRaised, 5),
            Show(EnemyFrameId.DragonWingFacingLeftLowered, 5),
        ],
        [P.IdleFacingRight] = [Hold(EnemyFrameId.DragonBodyFacingRight)],
        [P.WingsFacingRight] =
        [
            Show(EnemyFrameId.DragonWingFacingRightRaised, 5),
            Show(EnemyFrameId.DragonWingFacingRightLowered, 5),
        ],
        [P.AttackingFacingLeft] =
        [
            Show(EnemyFrameId.DragonSpittingFacingLeftHeadBack, 32),
            Show(EnemyFrameId.DragonSpittingFacingLeftHeadRaised, 3),
            Show(EnemyFrameId.DragonSpittingFacingLeftHeadForward, 7),
            Show(EnemyFrameId.DragonSpittingFacingLeftHeadRaised, 3),
            Show(EnemyFrameId.DragonSpittingFacingLeftHeadBack, 1),
            Cue(new(K.FinishAttack)),
        ],
        [P.AttackingFacingRight] =
        [
            Show(EnemyFrameId.DragonSpittingFacingRightHeadBack, 32),
            Show(EnemyFrameId.DragonSpittingFacingRightHeadRaised, 3),
            Show(EnemyFrameId.DragonSpittingFacingRightHeadForward, 7),
            Show(EnemyFrameId.DragonSpittingFacingRightHeadRaised, 3),
            Show(EnemyFrameId.DragonSpittingFacingRightHeadBack, 1),
            Cue(new(K.FinishAttack)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DragonSegment, Animation<DragonSegment>> StartingAt { get; } = new Dictionary<DragonSegment, Animation<DragonSegment>>
    {
        [P.IdleFacingLeft] = new([Whole(P.IdleFacingLeft)], AnimationEnd.Holds),
        [P.WingsFacingLeft] = new([Whole(P.WingsFacingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.IdleFacingRight] = new([Whole(P.IdleFacingRight)], AnimationEnd.Holds),
        [P.WingsFacingRight] = new([Whole(P.WingsFacingRight)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingFacingLeft] = new([Whole(P.AttackingFacingLeft)], AnimationEnd.Holds),
        [P.AttackingFacingRight] = new([Whole(P.AttackingFacingRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DragonSegment> StartingWith(DragonSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<DragonSegment> Whole(DragonSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, DragonCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, DragonCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DragonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DragonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DragonCue> Cue(DragonCue cue) => new AnimationBeat<EnemyFrameId, DragonCue>.Event(cue);
}
