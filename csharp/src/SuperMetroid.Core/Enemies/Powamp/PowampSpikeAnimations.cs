using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Powamp.PowampSpikeProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The Powamp spike's programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class PowampSpikeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PowampSpikeProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<PowampSpikeProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.PowampSpikeInitialFirst, 6),
            Show(EnemyProjectileFrameId.PowampSpikeInitialSecond, 6),
            Show(EnemyProjectileFrameId.PowampSpikeInitialThird, 6),
        ],
        [P.Delete] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PowampSpikeProgram, Animation<PowampSpikeProgram>> StartingAt { get; } = new Dictionary<PowampSpikeProgram, Animation<PowampSpikeProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([new(P.Delete, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PowampSpikeProgram> StartingWith(PowampSpikeProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
