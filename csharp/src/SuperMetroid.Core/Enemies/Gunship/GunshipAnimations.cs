using P = SuperMetroid.Core.Enemies.Gunship.GunshipProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class GunshipAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<GunshipProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<GunshipProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.EntrancePadOpening] =
        [
            Show(EnemyFrameId.GunshipEntrance00, 40),
            Show(EnemyFrameId.GunshipEntrance01, 8),
            Show(EnemyFrameId.GunshipEntrance02, 8),
            Show(EnemyFrameId.GunshipEntrance03, 8),
            Show(EnemyFrameId.GunshipEntrance04, 24),
            Show(EnemyFrameId.GunshipEntrance05, 8),
            Show(EnemyFrameId.GunshipEntrance06, 7),
            Show(EnemyFrameId.GunshipEntrance07, 6),
            Show(EnemyFrameId.GunshipEntrance08, 5),
            Show(EnemyFrameId.GunshipEntrance09, 4),
        ],
        [P.EntrancePadOpen] =
        [
            Show(EnemyFrameId.GunshipEntrance10, 4),
        ],
        [P.EntrancePadClosing] =
        [
            Show(EnemyFrameId.GunshipEntrance09, 4),
            Show(EnemyFrameId.GunshipEntrance08, 5),
            Show(EnemyFrameId.GunshipEntrance07, 6),
            Show(EnemyFrameId.GunshipEntrance06, 7),
            Show(EnemyFrameId.GunshipEntrance05, 8),
            Show(EnemyFrameId.GunshipEntrance04, 24),
            Show(EnemyFrameId.GunshipEntrance03, 8),
            Show(EnemyFrameId.GunshipEntrance01, 8),
        ],
        [P.BottomEntrancePad] =
        [
            Show(EnemyFrameId.GunshipEntrance00, 8),
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
