using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MaridiaLargeSnailCue"/> with its typed operands.
/// </summary>
internal static class MaridiaLargeSnailAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MaridiaLargeSnailProgram, EnemyFrameId, MaridiaLargeSnailCue> Segments { get; } = new(
        new Dictionary<MaridiaLargeSnailProgram, AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Show(EnemyFrameId.OumFacingLeftIdle, 1),
        ],
        [P.FacingLeftAttacking] =
        [
            Show(EnemyFrameId.OumFacingLeftIdle, 16),
            Show(EnemyFrameId.OumFacingLeftAttackingSecond, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumFacingLeftAttackingThird, 16),
            Show(EnemyFrameId.OumFacingLeftAttackingFourth, 2),
            Show(EnemyFrameId.OumFacingLeftAttackingFifth, 3),
            Show(EnemyFrameId.OumFacingLeftAttackingSixth, 4),
            Show(EnemyFrameId.OumFacingLeftAttackingSeventh, 2),
            Show(EnemyFrameId.OumFacingLeftAttackingEighth, 3),
            Show(EnemyFrameId.OumFacingLeftAttackingSeventh, 1),
            Show(EnemyFrameId.OumFacingLeftAttackingSixth, 3),
            Show(EnemyFrameId.OumFacingLeftAttackingFifth, 2),
            Show(EnemyFrameId.OumFacingLeftAttackingFourth, 1),
            Show(EnemyFrameId.OumFacingLeftAttackingThird, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingLeftRollingForwards] =
        [
            Show(EnemyFrameId.OumFacingLeftAttackingThird, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSecond, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsThird, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsFourth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsFifth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSixth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSeventh, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsEighth, 7),
        ],
        [P.FacingLeftRollingBackwards] =
        [
            Show(EnemyFrameId.OumFacingLeftRollingForwardsEighth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSeventh, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSixth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsFifth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsFourth, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsThird, 7),
            Show(EnemyFrameId.OumFacingLeftRollingForwardsSecond, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftAttackingThird, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
        [P.FacingRightIdle] =
        [
            Show(EnemyFrameId.OumFacingRightIdle, 1),
        ],
        [P.FacingRightAttacking] =
        [
            Show(EnemyFrameId.OumFacingRightIdle, 16),
            Show(EnemyFrameId.OumFacingRightAttackingSecond, 16),
            Show(EnemyFrameId.OumFacingRightAttackingThird, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumFacingRightAttackingFourth, 2),
            Show(EnemyFrameId.OumFacingRightAttackingFifth, 3),
            Show(EnemyFrameId.OumFacingRightAttackingSixth, 4),
            Show(EnemyFrameId.OumFacingRightAttackingSeventh, 2),
            Show(EnemyFrameId.OumFacingRightAttackingEighth, 3),
            Show(EnemyFrameId.OumFacingRightAttackingSeventh, 1),
            Show(EnemyFrameId.OumFacingRightAttackingSixth, 3),
            Show(EnemyFrameId.OumFacingRightAttackingFifth, 2),
            Show(EnemyFrameId.OumFacingRightAttackingFourth, 1),
            Show(EnemyFrameId.OumFacingRightAttackingThird, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingRightRollingForwards] =
        [
            Show(EnemyFrameId.OumFacingRightAttackingThird, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSecond, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightRollingForwardsThird, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsFourth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsFifth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSixth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSeventh, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsEighth, 7),
        ],
        [P.FacingRightRollingBackwards] =
        [
            Show(EnemyFrameId.OumFacingRightRollingForwardsEighth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSeventh, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSixth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsFifth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsFourth, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsThird, 7),
            Show(EnemyFrameId.OumFacingRightRollingForwardsSecond, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightAttackingThird, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MaridiaLargeSnailProgram, Animation<MaridiaLargeSnailProgram>> StartingAt { get; } = new Dictionary<MaridiaLargeSnailProgram, Animation<MaridiaLargeSnailProgram>>
    {
        [P.FacingLeftIdle] = new([new(P.FacingLeftIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftAttacking] = new([new(P.FacingLeftAttacking, 0, 15, 1)], AnimationEnd.Holds),
        [P.FacingLeftRollingForwards] = new([new(P.FacingLeftRollingForwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRollingBackwards] = new([new(P.FacingLeftRollingBackwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightIdle] = new([new(P.FacingRightIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightAttacking] = new([new(P.FacingRightAttacking, 0, 15, 1)], AnimationEnd.Holds),
        [P.FacingRightRollingForwards] = new([new(P.FacingRightRollingForwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRollingBackwards] = new([new(P.FacingRightRollingBackwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MaridiaLargeSnailProgram> StartingWith(MaridiaLargeSnailProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Cue(MaridiaLargeSnailCue cue) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Event(cue);
}
