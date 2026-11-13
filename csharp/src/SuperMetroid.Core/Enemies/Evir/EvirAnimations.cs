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
            Show(EnemyFrameId.EvirBodyFacingLeftFirst, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftSecond, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftThird, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftFourth, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftFifth, 10),
            Show(EnemyFrameId.EvirBodyFacingLeftSixth, 10),
        ],
        [P.ArmsFacingLeft] =
        [
            Show(EnemyFrameId.EvirArmsFacingLeftFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftThird, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftThird, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftThird, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftThird, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingLeftFifth, 48),
        ],
        [P.BodyFacingRight] =
        [
            Show(EnemyFrameId.EvirBodyFacingRightFirst, 10),
            Show(EnemyFrameId.EvirBodyFacingRightSecond, 10),
            Show(EnemyFrameId.EvirBodyFacingRightThird, 10),
            Show(EnemyFrameId.EvirBodyFacingRightFourth, 10),
            Show(EnemyFrameId.EvirBodyFacingRightFifth, 10),
            Show(EnemyFrameId.EvirBodyFacingRightSixth, 10),
        ],
        [P.ArmsFacingRight] =
        [
            Show(EnemyFrameId.EvirArmsFacingRightFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingRightThird, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingRightThird, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingRightThird, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFirst, 10),
            Show(EnemyFrameId.EvirArmsFacingRightSecond, 10),
            Show(EnemyFrameId.EvirArmsFacingRightThird, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFourth, 10),
            Show(EnemyFrameId.EvirArmsFacingRightFifth, 48),
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
