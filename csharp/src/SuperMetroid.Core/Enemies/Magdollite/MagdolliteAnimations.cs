using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="MagdolliteCue"/> with its typed operands.
/// </summary>
internal static class MagdolliteAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteProgram, EnemyFrameId, MagdolliteCue> Segments { get; } = new(
        new Dictionary<MagdolliteProgram, AnimationBeat<EnemyFrameId, MagdolliteCue>[]>
    {
        [P.LeftIdle] =
        [
            Show(EnemyFrameId.MagdolliteLeftIdle0, 13),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 13),
            Show(EnemyFrameId.MagdolliteLeftIdle2, 13),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 13),
        ],
        [P.LeftThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteLeftThrow0, 26),
            Show(EnemyFrameId.MagdolliteLeftThrow1, 8),
            Cue(new(K.ShiftLeftUp4)),
            Show(EnemyFrameId.MagdolliteLeftThrow2, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteLeftThrow2, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteLeftThrow2, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftLeftUp4Again)),
            Show(EnemyFrameId.MagdolliteLeftThrow3, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.LeftSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmerge1, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmerge2, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteLeftSubmerge2, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.LeftRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(EnemyFrameId.MagdolliteLeftSubmerge0, 1),
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
            Show(EnemyFrameId.MagdolliteLeftSubmerge0, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteLeftSubmerge2, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteLeftSubmerge2, 5),
            Show(EnemyFrameId.MagdolliteLeftSubmerge1, 5),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteLeftIdle1, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightIdle] =
        [
            Show(EnemyFrameId.MagdolliteRightIdle0, 13),
            Show(EnemyFrameId.MagdolliteRightIdle1, 13),
            Show(EnemyFrameId.MagdolliteRightIdle2, 13),
            Show(EnemyFrameId.MagdolliteRightIdle1, 13),
        ],
        [P.RightThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(EnemyFrameId.MagdolliteRightThrow0, 26),
            Show(EnemyFrameId.MagdolliteRightThrow1, 8),
            Cue(new(K.ShiftRightUp4)),
            Show(EnemyFrameId.MagdolliteRightThrow2, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteRightThrow2, 5),
            Cue(new(K.SpawnLava)),
            Show(EnemyFrameId.MagdolliteRightThrow2, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftRightUp8)),
            Show(EnemyFrameId.MagdolliteRightThrow3, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(EnemyFrameId.MagdolliteRightIdle1, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteRightIdle1, 5),
            Show(EnemyFrameId.MagdolliteRightSubmerge1, 5),
            Show(EnemyFrameId.MagdolliteRightSubmerge2, 5),
            Cue(new(K.MoveDown2)),
            Show(EnemyFrameId.MagdolliteRightSubmerge2, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.RightRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(EnemyFrameId.MagdolliteRightSubmerge0, 1),
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
            Show(EnemyFrameId.MagdolliteRightSubmerge0, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(EnemyFrameId.MagdolliteRightSubmerge2, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteRightSubmerge2, 5),
            Show(EnemyFrameId.MagdolliteRightSubmerge1, 5),
            Show(EnemyFrameId.MagdolliteRightIdle1, 5),
            Cue(new(K.MoveUp2)),
            Show(EnemyFrameId.MagdolliteRightIdle1, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.PillarPhase0] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase0, 1),
        ],
        [P.PillarPhase1] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase1, 1),
        ],
        [P.PillarPhase2] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase2, 1),
        ],
        [P.PillarPhase3] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase3, 1),
        ],
        [P.PillarPhase4] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase4, 1),
        ],
        [P.PillarPhase5] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase5, 1),
        ],
        [P.PillarPhase6] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase6, 1),
        ],
        [P.PillarPhase7] =
        [
            Show(EnemyFrameId.MagdollitePillarPhase7, 1),
        ],
        [P.PillarCap] =
        [
            Show(EnemyFrameId.MagdollitePillarCap, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MagdolliteProgram, Animation<MagdolliteProgram>> StartingAt { get; } = new Dictionary<MagdolliteProgram, Animation<MagdolliteProgram>>
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
    internal static Animation<MagdolliteProgram> StartingWith(MagdolliteProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Cue(MagdolliteCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Event(cue);
}
