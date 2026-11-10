using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Evir.EvirCueKind;
using P = SuperMetroid.Core.Enemies.Evir.EvirProgram;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations as typed programs (#641). Each
/// frame shows a <see cref="EvirFrame"/> pose and each callback becomes a <see cref="EvirCue"/> with its typed operands.
/// </summary>
internal static class EvirAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EvirProgram, EvirFrame, EvirCue> Segments { get; } = new(
        new Dictionary<EvirProgram, AnimationBeat<EvirFrame, EvirCue>[]>
    {
        [P.BodyFacingLeft] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
        ],
        [P.ArmsFacingLeft] =
        [
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 48),
        ],
        [P.BodyFacingRight] =
        [
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            Show(16, 10),
            Show(17, 10),
            Show(18, 10),
        ],
        [P.ArmsFacingRight] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(23, 48),
        ],
        [P.ProjectileNormal] =
        [
            Show(12, 1),
        ],
        [P.ProjectileRegenerating] =
        [
            Cue(new(K.SetInitialRegenerationOffset)),
            Cue(new(K.PlaySpitSound)),
        ],
        [P.ProjectileRegenerationLoop] =
        [
            Show(11, 8),
            Cue(new(K.AdvanceRegenerationOffset)),
            Show(12, 16),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EvirFrame, EvirCue> Show(int pose, int duration) => new AnimationBeat<EvirFrame, EvirCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EvirFrame, EvirCue> Cue(EvirCue cue) => new AnimationBeat<EvirFrame, EvirCue>.Event(cue);
}
