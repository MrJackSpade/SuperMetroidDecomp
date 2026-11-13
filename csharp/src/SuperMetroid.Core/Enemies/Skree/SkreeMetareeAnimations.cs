using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skree.SkreeMetareeCueKind;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="SkreeMetareeCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeProgram, EnemyFrameId, SkreeMetareeCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeProgram, AnimationBeat<EnemyFrameId, SkreeMetareeCue>[]>
    {
        [P.MetareeIdling] =
        [
            Show(EnemyFrameId.MetareeIdle0, 10),
            Show(EnemyFrameId.MetareeIdle1, 10),
            Show(EnemyFrameId.MetareeIdle2, 10),
            Show(EnemyFrameId.MetareeIdle3, 10),
        ],
        [P.MetareePreparingAttack] =
        [
            Show(EnemyFrameId.MetareeIdle0, 16),
            Show(EnemyFrameId.MetareePrepare1, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.MetareeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.MetareeDivingLoop] =
        [
            Show(EnemyFrameId.MetareeIdle1, 2),
            Show(EnemyFrameId.MetareeIdle2, 2),
            Show(EnemyFrameId.MetareeIdle3, 2),
            Show(EnemyFrameId.MetareeIdle0, 2),
        ],
        [P.MetareeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.MetareeIdle0, 1),
        ],
        [P.SkreeIdling] =
        [
            Show(EnemyFrameId.SkreeIdle0, 10),
            Show(EnemyFrameId.SkreeIdle1, 10),
            Show(EnemyFrameId.SkreeIdle2, 10),
            Show(EnemyFrameId.SkreeIdle3, 10),
        ],
        [P.SkreePreparingAttack] =
        [
            Show(EnemyFrameId.SkreeIdle0, 16),
            Show(EnemyFrameId.SkreePrepare1, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.SkreeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.SkreeDivingLoop] =
        [
            Show(EnemyFrameId.SkreeIdle1, 2),
            Show(EnemyFrameId.SkreeIdle2, 2),
            Show(EnemyFrameId.SkreeIdle3, 2),
            Show(EnemyFrameId.SkreeIdle0, 2),
        ],
        [P.SkreeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.SkreeIdle0, 1),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Event(cue);
}
