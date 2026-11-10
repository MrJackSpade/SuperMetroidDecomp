using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skree.SkreeMetareeCueKind;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeProgram;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641). Each
/// frame shows a <see cref="SkreeMetareeFrame"/> pose and each callback becomes a <see cref="SkreeMetareeCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeProgram, SkreeMetareeFrame, SkreeMetareeCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeProgram, AnimationBeat<SkreeMetareeFrame, SkreeMetareeCue>[]>
    {
        [P.MetareeIdling] =
        [
            Show(0, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
        ],
        [P.MetareePreparingAttack] =
        [
            Show(0, 16),
            Show(1, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.MetareeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.MetareeDivingLoop] =
        [
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(0, 2),
        ],
        [P.MetareeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(0, 1),
        ],
        [P.SkreeIdling] =
        [
            Show(5, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
        ],
        [P.SkreePreparingAttack] =
        [
            Show(5, 16),
            Show(6, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.SkreeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.SkreeDivingLoop] =
        [
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(5, 2),
        ],
        [P.SkreeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(5, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeProgram, Animation<SkreeMetareeProgram>> StartingAt { get; } = new Dictionary<SkreeMetareeProgram, Animation<SkreeMetareeProgram>>
    {
        [P.MetareeIdling] = new([new(P.MetareeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetareePreparingAttack] = new([new(P.MetareePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.MetareeDiving] = new([new(P.MetareeDiving, 0, 1, 1), new(P.MetareeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetareeDivingLoop] = new([new(P.MetareeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetareeStopAnimating] = new([new(P.MetareeStopAnimating, 0, 2, 1)], AnimationEnd.Holds),
        [P.SkreeIdling] = new([new(P.SkreeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SkreePreparingAttack] = new([new(P.SkreePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.SkreeDiving] = new([new(P.SkreeDiving, 0, 1, 1), new(P.SkreeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SkreeDivingLoop] = new([new(P.SkreeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SkreeStopAnimating] = new([new(P.SkreeStopAnimating, 0, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeProgram> StartingWith(SkreeMetareeProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SkreeMetareeFrame, SkreeMetareeCue> Show(int pose, int duration) => new AnimationBeat<SkreeMetareeFrame, SkreeMetareeCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SkreeMetareeFrame, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => new AnimationBeat<SkreeMetareeFrame, SkreeMetareeCue>.Event(cue);
}
