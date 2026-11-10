using P = SuperMetroid.Core.Enemies.Gunship.GunshipProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641). Each
/// frame shows a <see cref="GunshipFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class GunshipAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<GunshipProgram, GunshipFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<GunshipProgram, AnimationBeat<GunshipFrame, NoAnimationCue>[]>
    {
        [P.EntrancePadOpening] =
        [
            Show(12, 40),
            Show(11, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 24),
            Show(5, 8),
            Show(6, 7),
            Show(7, 6),
            Show(8, 5),
            Show(9, 4),
        ],
        [P.EntrancePadOpen] =
        [
            Show(10, 4),
        ],
        [P.EntrancePadClosing] =
        [
            Show(9, 4),
            Show(8, 5),
            Show(7, 6),
            Show(6, 7),
            Show(5, 8),
            Show(4, 24),
            Show(3, 8),
            Show(11, 8),
        ],
        [P.BottomEntrancePad] =
        [
            Show(12, 8),
        ],
        [P.TopHull] =
        [
            Show(0, 1),
        ],
        [P.BottomHull] =
        [
            Show(1, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<GunshipProgram, Animation<GunshipProgram>> StartingAt { get; } = new Dictionary<GunshipProgram, Animation<GunshipProgram>>
    {
        [P.EntrancePadOpening] = new([new(P.EntrancePadOpening, 0, 10, 1), new(P.EntrancePadOpen, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.EntrancePadOpen] = new([new(P.EntrancePadOpen, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EntrancePadClosing] = new([new(P.EntrancePadClosing, 0, 8, 1), new(P.BottomEntrancePad, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BottomEntrancePad] = new([new(P.BottomEntrancePad, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TopHull] = new([new(P.TopHull, 0, 1, 1)], AnimationEnd.Holds),
        [P.BottomHull] = new([new(P.BottomHull, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipProgram> StartingWith(GunshipProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<GunshipFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<GunshipFrame, NoAnimationCue>.Frame(new(pose), duration);
}
