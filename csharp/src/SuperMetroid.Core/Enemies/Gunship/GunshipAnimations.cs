using P = SuperMetroid.Core.Enemies.Gunship.GunshipProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class GunshipAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<GunshipProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<GunshipProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.EntrancePadOpening] =
        [
            Show(EnemyFrameId.GunshipEntrancePadOpeningFirst, 40),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSecond, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningThird, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningFourth, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningFifth, 24),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSixth, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSeventh, 7),
            Show(EnemyFrameId.GunshipEntrancePadOpeningEighth, 6),
            Show(EnemyFrameId.GunshipEntrancePadOpeningNinth, 5),
            Show(EnemyFrameId.GunshipEntrancePadOpeningTenth, 4),
        ],
        [P.EntrancePadOpen] =
        [
            Show(EnemyFrameId.GunshipEntrancePadOpen, 4),
        ],
        [P.EntrancePadClosing] =
        [
            Show(EnemyFrameId.GunshipEntrancePadOpeningTenth, 4),
            Show(EnemyFrameId.GunshipEntrancePadOpeningNinth, 5),
            Show(EnemyFrameId.GunshipEntrancePadOpeningEighth, 6),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSeventh, 7),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSixth, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningFifth, 24),
            Show(EnemyFrameId.GunshipEntrancePadOpeningFourth, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpeningSecond, 8),
        ],
        [P.BottomEntrancePad] =
        [
            Show(EnemyFrameId.GunshipEntrancePadOpeningFirst, 8),
        ],
        [P.TopHull] =
        [
            Show(EnemyFrameId.GunshipTopHull, 1),
        ],
        [P.BottomHull] =
        [
            Show(EnemyFrameId.GunshipBottomHull, 1),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
