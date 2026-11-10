using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Corpses.DeadTourianCueKind;
using P = SuperMetroid.Core.Enemies.Corpses.DeadTourianProgram;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed programs (#641). Each
/// frame shows a <see cref="DeadTourianFrame"/> pose and each callback becomes a <see cref="DeadTourianCue"/> with its typed operands.
/// </summary>
internal static class DeadTourianAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DeadTourianProgram, DeadTourianFrame, DeadTourianCue> Segments { get; } = new(
        new Dictionary<DeadTourianProgram, AnimationBeat<DeadTourianFrame, DeadTourianCue>[]>
    {
        [P.SidehopperHopping] =
        [
            Show(10, 2),
            Show(11, 4),
            Show(10, 5),
            Show(11, 48),
            Show(10, 5),
            Show(11, 4),
            Show(10, 5),
            Show(11, 4),
            Cue(new(K.EndHop)),
        ],
        [P.SidehopperIdle] =
        [
            Show(12, 1),
        ],
        [P.SidehopperCorpse] =
        [
            Show(0, 1),
        ],
        [P.SidehopperInitiallyDead] =
        [
            Show(1, 1),
        ],
        [P.ZoomerCorpse0] =
        [
            Show(2, 1),
        ],
        [P.ZoomerCorpse1] =
        [
            Show(3, 1),
        ],
        [P.ZoomerCorpse2] =
        [
            Show(4, 1),
        ],
        [P.RipperCorpse0] =
        [
            Show(5, 1),
        ],
        [P.RipperCorpse1] =
        [
            Show(6, 1),
        ],
        [P.SkreeCorpse0] =
        [
            Show(7, 1),
        ],
        [P.SkreeCorpse1] =
        [
            Show(8, 1),
        ],
        [P.SkreeCorpse2] =
        [
            Show(9, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTourianProgram, Animation<DeadTourianProgram>> StartingAt { get; } = new Dictionary<DeadTourianProgram, Animation<DeadTourianProgram>>
    {
        [P.SidehopperHopping] = new([new(P.SidehopperHopping, 0, 9, 1)], AnimationEnd.Holds),
        [P.SidehopperIdle] = new([new(P.SidehopperIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.SidehopperCorpse] = new([new(P.SidehopperCorpse, 0, 1, 1)], AnimationEnd.Holds),
        [P.SidehopperInitiallyDead] = new([new(P.SidehopperInitiallyDead, 0, 1, 1)], AnimationEnd.Holds),
        [P.ZoomerCorpse0] = new([new(P.ZoomerCorpse0, 0, 1, 1)], AnimationEnd.Holds),
        [P.ZoomerCorpse1] = new([new(P.ZoomerCorpse1, 0, 1, 1)], AnimationEnd.Holds),
        [P.ZoomerCorpse2] = new([new(P.ZoomerCorpse2, 0, 1, 1)], AnimationEnd.Holds),
        [P.RipperCorpse0] = new([new(P.RipperCorpse0, 0, 1, 1)], AnimationEnd.Holds),
        [P.RipperCorpse1] = new([new(P.RipperCorpse1, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkreeCorpse0] = new([new(P.SkreeCorpse0, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkreeCorpse1] = new([new(P.SkreeCorpse1, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkreeCorpse2] = new([new(P.SkreeCorpse2, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTourianProgram> StartingWith(DeadTourianProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<DeadTourianFrame, DeadTourianCue> Show(int pose, int duration) => new AnimationBeat<DeadTourianFrame, DeadTourianCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<DeadTourianFrame, DeadTourianCue> Cue(DeadTourianCue cue) => new AnimationBeat<DeadTourianFrame, DeadTourianCue>.Event(cue);
}
