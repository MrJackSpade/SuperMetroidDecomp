using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProgram;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="CacatacFrame"/> pose and each callback becomes a <see cref="CacatacCue"/> with its typed operands.
/// </summary>
internal static class CacatacAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<CacatacProgram, CacatacFrame, CacatacCue> Programs { get; } = new(
        new Dictionary<CacatacProgram, AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>[]>
    {
        // $A2:9E8A
        [P.UpsideUpIdle] =
        [
            Cue(new(K.RestorePatrol)),
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 8),
            GoTo(P.UpsideUpIdle),
        ],
        // $A2:9EB0
        [P.UpsideUpAttack] =
        [
            Show(0, 21),
            Show(8, 5),
            Show(9, 21),
            Show(8, 5),
            Cue(new(K.PlaySpikeSound)),
            Cue(new(K.SpawnSpike, Value: 0)),
            Cue(new(K.SpawnSpike, Value: 12)),
            Cue(new(K.SpawnSpike, Value: 2)),
            Cue(new(K.SpawnSpike, Value: 14)),
            Cue(new(K.SpawnSpike, Value: 4)),
            GoTo(P.UpsideUpIdle),
        ],
        // $A2:9EDA
        [P.UpsideDownIdle] =
        [
            Cue(new(K.RestorePatrol)),
            FallInto(P.UpsideDownIdleLoop),
        ],
        // $A2:9EDC
        [P.UpsideDownIdleLoop] =
        [
            Show(10, 8),
            Show(11, 8),
            Show(12, 8),
            Show(13, 8),
            Show(14, 8),
            Show(15, 8),
            Show(16, 8),
            Show(17, 8),
            GoTo(P.UpsideDownIdleLoop),
        ],
        // $A2:9F00
        [P.UpsideDownAttack] =
        [
            Show(10, 21),
            Show(18, 5),
            Show(19, 21),
            Show(18, 5),
            Cue(new(K.PlaySpikeSound)),
            Cue(new(K.SpawnSpike, Value: 6)),
            Cue(new(K.SpawnSpike, Value: 16)),
            Cue(new(K.SpawnSpike, Value: 8)),
            Cue(new(K.SpawnSpike, Value: 18)),
            Cue(new(K.SpawnSpike, Value: 10)),
            GoTo(P.UpsideDownIdle),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params CacatacProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => CacatacFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue> Show(int pose, int duration) => AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue> Cue(CacatacCue cue) => AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue> GoTo(CacatacProgram program) => AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue> FallInto(CacatacProgram program) => AnimationProgramStep<CacatacProgram, CacatacFrame, CacatacCue>.FallInto(program);

}
