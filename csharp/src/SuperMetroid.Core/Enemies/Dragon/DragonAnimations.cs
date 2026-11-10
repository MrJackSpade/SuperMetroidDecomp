using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Dragon.DragonCueKind;
using P = SuperMetroid.Core.Enemies.Dragon.DragonProgram;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations as typed programs (#641). Each
/// frame shows a <see cref="DragonFrame"/> pose and each callback becomes a <see cref="DragonCue"/> with its typed operands.
/// </summary>
internal static class DragonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DragonProgram, DragonFrame, DragonCue> Segments { get; } = new(
        new Dictionary<DragonProgram, AnimationBeat<DragonFrame, DragonCue>[]>
    {
        [P.IdleFacingLeft] =
        [
            Show(0, 1),
        ],
        [P.WingsFacingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
        ],
        [P.IdleFacingRight] =
        [
            Show(6, 1),
        ],
        [P.WingsFacingRight] =
        [
            Show(10, 5),
            Show(11, 5),
        ],
        [P.AttackingFacingLeft] =
        [
            Show(1, 32),
            Show(2, 3),
            Show(3, 7),
            Show(2, 3),
            Show(1, 1),
            Cue(new(K.FinishAttack)),
        ],
        [P.AttackingFacingRight] =
        [
            Show(7, 32),
            Show(8, 3),
            Show(9, 7),
            Show(8, 3),
            Show(7, 1),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DragonFrame, DragonCue> Show(int pose, int duration) => new AnimationBeat<DragonFrame, DragonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<DragonFrame, DragonCue> Cue(DragonCue cue) => new AnimationBeat<DragonFrame, DragonCue>.Event(cue);
}
