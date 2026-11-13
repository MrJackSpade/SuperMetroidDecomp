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
            Show(EnemyFrameId.MagdolliteLeftIdleFirst, 13),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 13),
            Show(EnemyFrameId.MagdolliteLeftIdleThird, 13),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 13),
        ],
        [P.LeftThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteLeftThrowFirst, 26),
            Show(EnemyFrameId.MagdolliteLeftThrowSecond, 8),
            Cue(new(K.ShiftLeftUp4)),
            Show(EnemyFrameId.MagdolliteLeftThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteLeftThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteLeftThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftLeftUp4Again)),
            Show(EnemyFrameId.MagdolliteLeftThrowFourth, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.LeftSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmergeSecond, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmergeThird, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteLeftSubmergeThird, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.LeftRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(EnemyFrameId.MagdolliteLeftRaisePillar, 1),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.LeftEmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.RestoreOrigins)),
        ],
        [P.LeftLowerPillar] =
        [
            Cue(new(K.MoveWithPillarDown1)),
            Show(EnemyFrameId.MagdolliteLeftRaisePillar, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteLeftSubmergeThird, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteLeftSubmergeThird, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmergeSecond, 5),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteLeftIdleSecond, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightIdle] =
        [
            Show(EnemyFrameId.MagdolliteRightIdleFirst, 13),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 13),
            Show(EnemyFrameId.MagdolliteRightIdleThird, 13),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 13),
        ],
        [P.RightThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRightThrowFirst, 26),
            Show(EnemyFrameId.MagdolliteRightThrowSecond, 8),
            Cue(new(K.ShiftRightUp4)),
            Show(EnemyFrameId.MagdolliteRightThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteRightThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteRightThrowThird, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftRightUp8)),
            Show(EnemyFrameId.MagdolliteRightThrowFourth, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 5),
            Show(EnemyFrameId.MagdolliteRightSubmergeSecond, 5),
            Show(EnemyFrameId.MagdolliteRightSubmergeThird, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteRightSubmergeThird, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.RightRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(EnemyFrameId.MagdolliteRightRaisePillar, 1),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightEmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.RestoreOrigins)),
        ],
        [P.RightLowerPillar] =
        [
            Cue(new(K.MoveWithPillarDown1)),
            Show(EnemyFrameId.MagdolliteRightRaisePillar, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteRightSubmergeThird, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteRightSubmergeThird, 5),
            Show(EnemyFrameId.MagdolliteRightSubmergeSecond, 5),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteRightIdleSecond, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.PillarPhase0] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseFirst, 1),
        ],
        [P.PillarPhase1] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseSecond, 1),
        ],
        [P.PillarPhase2] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseThird, 1),
        ],
        [P.PillarPhase3] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseFourth, 1),
        ],
        [P.PillarPhase4] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseFifth, 1),
        ],
        [P.PillarPhase5] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseSixth, 1),
        ],
        [P.PillarPhase6] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseSeventh, 1),
        ],
        [P.PillarPhase7] =
        [
            Show(EnemyFrameId.MagdollitePillarPhaseEighth, 1),
        ],
        [P.PillarCap] =
        [
            Show(EnemyFrameId.MagdollitePillarCap, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MagdolliteSegment, Animation<MagdolliteSegment>> StartingAt { get; } = new Dictionary<MagdolliteSegment, Animation<MagdolliteSegment>>
    {
        [P.LeftIdle] = new([new(P.LeftIdle, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LeftThrow] = new([new(P.LeftThrow, 0, 15, 1)], AnimationEnd.Holds),
        [P.LeftSubmerge] = new([new(P.LeftSubmerge, 0, 9, 1), new(P.LeftRaisePillar, 0, 2, 24), new(P.LeftRaisePillar, 2, 1, 1)], AnimationEnd.Holds),
        [P.LeftEmerge] = new([new(P.LeftEmerge, 0, 2, 1), new(P.LeftLowerPillar, 0, 2, 24), new(P.LeftLowerPillar, 2, 9, 1)], AnimationEnd.Holds),
        [P.RightIdle] = new([new(P.RightIdle, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightThrow] = new([new(P.RightThrow, 0, 15, 1)], AnimationEnd.Holds),
        [P.RightSubmerge] = new([new(P.RightSubmerge, 0, 9, 1), new(P.RightRaisePillar, 0, 2, 24), new(P.RightRaisePillar, 2, 1, 1)], AnimationEnd.Holds),
        [P.RightEmerge] = new([new(P.RightEmerge, 0, 2, 1), new(P.RightLowerPillar, 0, 2, 24), new(P.RightLowerPillar, 2, 9, 1)], AnimationEnd.Holds),
        [P.PillarPhase0] = new([new(P.PillarPhase0, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase1] = new([new(P.PillarPhase1, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase2] = new([new(P.PillarPhase2, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase3] = new([new(P.PillarPhase3, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase4] = new([new(P.PillarPhase4, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase5] = new([new(P.PillarPhase5, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase6] = new([new(P.PillarPhase6, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarPhase7] = new([new(P.PillarPhase7, 0, 1, 1)], AnimationEnd.Holds),
        [P.PillarCap] = new([new(P.PillarCap, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteSegment> StartingWith(MagdolliteSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Cue(MagdolliteCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Event(cue);
}
