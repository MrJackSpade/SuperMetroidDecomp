using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Dragon.DragonCueKind;
using P = SuperMetroid.Core.Enemies.Dragon.DragonProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="DragonCue"/> with its typed operands.
/// </summary>
internal static class DragonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DragonProgram, EnemyFrameId, DragonCue> Segments { get; } = new(
        new Dictionary<DragonProgram, AnimationBeat<EnemyFrameId, DragonCue>[]>
    {
        [P.IdleFacingLeft] =
        [
            Show(EnemyFrameId.DragonBodyIdleLeft, 1),
        ],
        [P.WingsFacingLeft] =
        [
            Show(EnemyFrameId.DragonWingLeft0, 5),
            Show(EnemyFrameId.DragonWingLeft1, 5),
        ],
        [P.IdleFacingRight] =
        [
            Show(EnemyFrameId.DragonBodyIdleRight, 1),
        ],
        [P.WingsFacingRight] =
        [
            Show(EnemyFrameId.DragonWingRight0, 5),
            Show(EnemyFrameId.DragonWingRight1, 5),
        ],
        [P.AttackingFacingLeft] =
        [
            Show(EnemyFrameId.DragonBodyAttackLeft0, 32),
            Show(EnemyFrameId.DragonBodyAttackLeft1, 3),
            Show(EnemyFrameId.DragonBodyAttackLeft2, 7),
            Show(EnemyFrameId.DragonBodyAttackLeft1, 3),
            Show(EnemyFrameId.DragonBodyAttackLeft0, 1),
            Cue(new(K.FinishAttack)),
        ],
        [P.AttackingFacingRight] =
        [
            Show(EnemyFrameId.DragonBodyAttackRight0, 32),
            Show(EnemyFrameId.DragonBodyAttackRight1, 3),
            Show(EnemyFrameId.DragonBodyAttackRight2, 7),
            Show(EnemyFrameId.DragonBodyAttackRight1, 3),
            Show(EnemyFrameId.DragonBodyAttackRight0, 1),
            Cue(new(K.FinishAttack)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DragonProgram, Animation<DragonProgram>> StartingAt { get; } = new Dictionary<DragonProgram, Animation<DragonProgram>>
    {
        [P.IdleFacingLeft] = new([new(P.IdleFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.WingsFacingLeft] = new([new(P.WingsFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleFacingRight] = new([new(P.IdleFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.WingsFacingRight] = new([new(P.WingsFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingFacingLeft] = new([new(P.AttackingFacingLeft, 0, 6, 1)], AnimationEnd.Holds),
        [P.AttackingFacingRight] = new([new(P.AttackingFacingRight, 0, 6, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DragonProgram> StartingWith(DragonProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DragonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DragonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DragonCue> Cue(DragonCue cue) => new AnimationBeat<EnemyFrameId, DragonCue>.Event(cue);
}
