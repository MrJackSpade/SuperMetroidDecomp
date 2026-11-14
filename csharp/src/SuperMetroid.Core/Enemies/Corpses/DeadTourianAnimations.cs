using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Corpses.DeadTourianCueKind;
using P = SuperMetroid.Core.Enemies.Corpses.DeadTourianSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="DeadTourianCue"/> naming what it does.
/// </summary>
internal static class DeadTourianAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DeadTourianSegment, EnemyFrameId, DeadTourianCue> Segments { get; } = new(
        new Dictionary<DeadTourianSegment, AnimationBeat<EnemyFrameId, DeadTourianCue>[]>
    {
        [P.SidehopperHopping] =
        [
            Show(EnemyFrameId.DeadSidehopperHopBody, 2),
            Show(EnemyFrameId.DeadSidehopperHopCrouched, 4),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopCrouched, 48),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopCrouched, 4),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopCrouched, 4),
            Cue(new(K.EndHop)),
        ],
        [P.SidehopperIdle] =
        [
            Show(EnemyFrameId.DeadSidehopperIdle, 1),
        ],
        [P.SidehopperCorpse] =
        [
            Show(EnemyFrameId.DeadSidehopperCorpse, 1),
        ],
        [P.SidehopperInitiallyDead] =
        [
            Show(EnemyFrameId.DeadSidehopperInitiallyDead, 1),
        ],
        [P.ZoomerCorpseOnItsBack] =
        [
            Show(EnemyFrameId.DeadZoomerCorpseOnItsBack, 1),
        ],
        [P.ZoomerCorpseUpright] =
        [
            Show(EnemyFrameId.DeadZoomerCorpseUpright, 1),
        ],
        [P.RipperCorpseLevel] =
        [
            Show(EnemyFrameId.DeadRipperCorpseLevel, 1),
        ],
        [P.RipperCorpseTilted] =
        [
            Show(EnemyFrameId.DeadRipperCorpseTilted, 1),
        ],
        [P.SkreeCorpse] =
        [
            Show(EnemyFrameId.DeadSkreeCorpse, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTourianSegment, Animation<DeadTourianSegment>> StartingAt { get; } = new Dictionary<DeadTourianSegment, Animation<DeadTourianSegment>>
    {
        [P.SidehopperHopping] = new([new(P.SidehopperHopping, 0, 9, 1)], AnimationEnd.Holds),
        [P.SidehopperIdle] = new([new(P.SidehopperIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.SidehopperCorpse] = new([new(P.SidehopperCorpse, 0, 1, 1)], AnimationEnd.Holds),
        [P.SidehopperInitiallyDead] = new([new(P.SidehopperInitiallyDead, 0, 1, 1)], AnimationEnd.Holds),
        [P.ZoomerCorpseOnItsBack] = new([new(P.ZoomerCorpseOnItsBack, 0, 1, 1)], AnimationEnd.Holds),
        [P.ZoomerCorpseUpright] = new([new(P.ZoomerCorpseUpright, 0, 1, 1)], AnimationEnd.Holds),
        [P.RipperCorpseLevel] = new([new(P.RipperCorpseLevel, 0, 1, 1)], AnimationEnd.Holds),
        [P.RipperCorpseTilted] = new([new(P.RipperCorpseTilted, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkreeCorpse] = new([new(P.SkreeCorpse, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTourianSegment> StartingWith(DeadTourianSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Cue(DeadTourianCue cue) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Event(cue);
}
