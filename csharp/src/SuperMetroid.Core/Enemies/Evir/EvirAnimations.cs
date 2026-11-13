using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="EvirCue"/> with its typed operands.
/// </summary>
internal static class EvirAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EvirProgram, EnemyFrameId, EvirCue> Segments { get; } = new(
        new Dictionary<EvirProgram, AnimationBeat<EnemyFrameId, EvirCue>[]>
    {
        [P.BodyFacingLeft] =
        [
            Show(EnemyFrameId.EvirBodyLeft0, 10),
            Show(EnemyFrameId.EvirBodyLeft1, 10),
            Show(EnemyFrameId.EvirBodyLeft2, 10),
            Show(EnemyFrameId.EvirBodyLeft3, 10),
            Show(EnemyFrameId.EvirBodyLeft4, 10),
            Show(EnemyFrameId.EvirBodyLeft5, 10),
        ],
        [P.ArmsFacingLeft] =
        [
            Show(EnemyFrameId.EvirArmsLeft0, 10),
            Show(EnemyFrameId.EvirArmsLeft1, 10),
            Show(EnemyFrameId.EvirArmsLeft2, 10),
            Show(EnemyFrameId.EvirArmsLeft3, 10),
            Show(EnemyFrameId.EvirArmsLeft0, 10),
            Show(EnemyFrameId.EvirArmsLeft1, 10),
            Show(EnemyFrameId.EvirArmsLeft2, 10),
            Show(EnemyFrameId.EvirArmsLeft3, 10),
            Show(EnemyFrameId.EvirArmsLeft0, 10),
            Show(EnemyFrameId.EvirArmsLeft1, 10),
            Show(EnemyFrameId.EvirArmsLeft2, 10),
            Show(EnemyFrameId.EvirArmsLeft3, 10),
            Show(EnemyFrameId.EvirArmsLeft0, 10),
            Show(EnemyFrameId.EvirArmsLeft1, 10),
            Show(EnemyFrameId.EvirArmsLeft2, 10),
            Show(EnemyFrameId.EvirArmsLeft3, 10),
            Show(EnemyFrameId.EvirArmsLeft4, 48),
        ],
        [P.BodyFacingRight] =
        [
            Show(EnemyFrameId.EvirBodyRight0, 10),
            Show(EnemyFrameId.EvirBodyRight1, 10),
            Show(EnemyFrameId.EvirBodyRight2, 10),
            Show(EnemyFrameId.EvirBodyRight3, 10),
            Show(EnemyFrameId.EvirBodyRight4, 10),
            Show(EnemyFrameId.EvirBodyRight5, 10),
        ],
        [P.ArmsFacingRight] =
        [
            Show(EnemyFrameId.EvirArmsRight0, 10),
            Show(EnemyFrameId.EvirArmsRight1, 10),
            Show(EnemyFrameId.EvirArmsRight2, 10),
            Show(EnemyFrameId.EvirArmsRight3, 10),
            Show(EnemyFrameId.EvirArmsRight0, 10),
            Show(EnemyFrameId.EvirArmsRight1, 10),
            Show(EnemyFrameId.EvirArmsRight2, 10),
            Show(EnemyFrameId.EvirArmsRight3, 10),
            Show(EnemyFrameId.EvirArmsRight0, 10),
            Show(EnemyFrameId.EvirArmsRight1, 10),
            Show(EnemyFrameId.EvirArmsRight2, 10),
            Show(EnemyFrameId.EvirArmsRight3, 10),
            Show(EnemyFrameId.EvirArmsRight0, 10),
            Show(EnemyFrameId.EvirArmsRight1, 10),
            Show(EnemyFrameId.EvirArmsRight2, 10),
            Show(EnemyFrameId.EvirArmsRight3, 10),
            Show(EnemyFrameId.EvirArmsRight4, 48),
        ],
        [P.ProjectileNormal] =
        [
            Show(EnemyFrameId.EvirProjectileFlight, 1),
        ],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
            Cue(new(K.PlaySpitSound)),
        ],
        [P.ProjectileRegenerationLoop] =
        [
            Show(EnemyFrameId.EvirProjectileRegenerating, 8),
            Cue(new(K.AdvanceRegenerationOffset)),
            Show(EnemyFrameId.EvirProjectileFlight, 16),
            Cue(new(K.FinishRegeneration)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EvirProgram, Animation<EvirProgram>> StartingAt { get; } = new Dictionary<EvirProgram, Animation<EvirProgram>>
    {
        [P.BodyFacingLeft] = new([new(P.BodyFacingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingLeft] = new([new(P.ArmsFacingLeft, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BodyFacingRight] = new([new(P.BodyFacingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ArmsFacingRight] = new([new(P.ArmsFacingRight, 0, 17, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ProjectileNormal] = new([new(P.ProjectileNormal, 0, 1, 1)], AnimationEnd.Holds),
        [P.ProjectileRegenerating] = new([new(P.ProjectileRegenerating, 0, 1, 1), new(P.ProjectileRegenerating, 1, 1, 1), new(P.ProjectileRegenerationLoop, 0, 2, 8), new(P.ProjectileRegenerationLoop, 2, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EvirProgram> StartingWith(EvirProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EvirCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EvirCue> Cue(EvirCue cue) => new AnimationBeat<EnemyFrameId, EvirCue>.Event(cue);
}
