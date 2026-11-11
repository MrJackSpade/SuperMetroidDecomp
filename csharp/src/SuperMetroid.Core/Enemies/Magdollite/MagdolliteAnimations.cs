using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteProgram;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations as typed programs (#641). Each
/// frame shows a <see cref="MagdolliteFrame"/> pose and each callback becomes a <see cref="MagdolliteCue"/> with its typed operands.
/// </summary>
internal static class MagdolliteAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> Segments { get; } = new(
        new Dictionary<MagdolliteProgram, AnimationBeat<MagdolliteFrame, MagdolliteCue>[]>
    {
        [P.LeftIdle] =
        [
            Show(0, 13),
            Show(1, 13),
            Show(2, 13),
            Show(1, 13),
        ],
        [P.LeftThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(3, 26),
            Show(4, 8),
            Cue(new(K.ShiftLeftUp4)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftLeftUp4Again)),
            Show(6, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.LeftSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(1, 5),
            Cue(new(K.MoveDown2)),
            Show(1, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.MoveDown2)),
            Show(10, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.LeftRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(8, 1),
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
            Show(8, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(10, 5),
            Cue(new(K.MoveUp2)),
            Show(10, 5),
            Show(9, 5),
            Show(1, 5),
            Cue(new(K.MoveUp2)),
            Show(1, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightIdle] =
        [
            Show(11, 13),
            Show(12, 13),
            Show(13, 13),
            Show(12, 13),
        ],
        [P.RightThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Sound: MagdolliteSoundDefinitions.Throw)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(14, 26),
            Show(15, 8),
            Cue(new(K.ShiftRightUp4)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftRightUp8)),
            Show(17, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.RightSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(12, 5),
            Cue(new(K.MoveDown2)),
            Show(12, 5),
            Show(19, 5),
            Show(20, 5),
            Cue(new(K.MoveDown2)),
            Show(20, 5),
            Cue(new(K.SinkAndShowParts)),
        ],
        [P.RightRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(18, 1),
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
            Show(18, 1),
            Cue(new(K.SurfaceAndHideParts)),
            Show(20, 5),
            Cue(new(K.MoveUp2)),
            Show(20, 5),
            Show(19, 5),
            Show(12, 5),
            Cue(new(K.MoveUp2)),
            Show(12, 5),
            Cue(new(K.ClearAnimationBusy)),
        ],
        [P.PillarPhase0] =
        [
            Show(21, 1),
        ],
        [P.PillarPhase1] =
        [
            Show(22, 1),
        ],
        [P.PillarPhase2] =
        [
            Show(23, 1),
        ],
        [P.PillarPhase3] =
        [
            Show(24, 1),
        ],
        [P.PillarPhase4] =
        [
            Show(25, 1),
        ],
        [P.PillarPhase5] =
        [
            Show(26, 1),
        ],
        [P.PillarPhase6] =
        [
            Show(27, 1),
        ],
        [P.PillarPhase7] =
        [
            Show(28, 1),
        ],
        [P.PillarCap] =
        [
            Show(7, 1),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MagdolliteFrame, MagdolliteCue> Show(int pose, int duration) => new AnimationBeat<MagdolliteFrame, MagdolliteCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MagdolliteFrame, MagdolliteCue> Cue(MagdolliteCue cue) => new AnimationBeat<MagdolliteFrame, MagdolliteCue>.Event(cue);
}
