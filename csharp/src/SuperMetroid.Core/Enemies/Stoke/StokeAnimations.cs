using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Stoke.StokeCueKind;
using P = SuperMetroid.Core.Enemies.Stoke.StokeProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's walking and fire-spitting animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="StokeCue"/> with its typed operands.
/// </summary>
internal static class StokeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<StokeProgram, EnemyFrameId, StokeCue> Segments { get; } = new(
        new Dictionary<StokeProgram, AnimationBeat<EnemyFrameId, StokeCue>[]>
    {
        [P.MovingLeft] =
        [
            Cue(new(K.SetMovingLeft)),
        ],
        [P.MovingLeftLoop] =
        [
            Show(EnemyFrameId.StokeWalkLeft0, 8),
            Show(EnemyFrameId.StokeWalkLeft1, 16),
            Show(EnemyFrameId.StokeWalkLeft2, 8),
            Show(EnemyFrameId.StokeWalkLeft3, 8),
        ],
        [P.AttackingLeft] =
        [
            Show(EnemyFrameId.StokeWalkLeft2, 16),
            Cue(new(K.SpawnFireball, FireballDirection: StokeDirection.Left)),
            Show(EnemyFrameId.StokeAttackLeft, 16),
        ],
        [P.MovingRight] =
        [
            Cue(new(K.SetMovingRight)),
        ],
        [P.MovingRightLoop] =
        [
            Show(EnemyFrameId.StokeWalkRight0, 8),
            Show(EnemyFrameId.StokeWalkRight1, 16),
            Show(EnemyFrameId.StokeWalkRight2, 8),
            Show(EnemyFrameId.StokeWalkRight3, 8),
        ],
        [P.AttackingRight] =
        [
            Show(EnemyFrameId.StokeWalkRight2, 16),
            Cue(new(K.SpawnFireball, FireballDirection: StokeDirection.Right)),
            Show(EnemyFrameId.StokeAttackRight, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<StokeProgram, Animation<StokeProgram>> StartingAt { get; } = new Dictionary<StokeProgram, Animation<StokeProgram>>
    {
        [P.MovingLeft] = new([new(P.MovingLeft, 0, 1, 1), new(P.MovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingLeftLoop] = new([new(P.MovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingLeft] = new([new(P.AttackingLeft, 0, 3, 1), new(P.MovingLeft, 0, 1, 1), new(P.MovingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
        [P.MovingRight] = new([new(P.MovingRight, 0, 1, 1), new(P.MovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MovingRightLoop] = new([new(P.MovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingRight] = new([new(P.AttackingRight, 0, 3, 1), new(P.MovingRight, 0, 1, 1), new(P.MovingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<StokeProgram> StartingWith(StokeProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, StokeCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, StokeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, StokeCue> Cue(StokeCue cue) => new AnimationBeat<EnemyFrameId, StokeCue>.Event(cue);
}
