using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Corpses.DeadTourianCueKind;
using P = SuperMetroid.Core.Enemies.Corpses.DeadTourianSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="DeadTourianCue"/> with its typed operands.
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
            Show(EnemyFrameId.DeadSidehopperHopAlt, 4),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopAlt, 48),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopAlt, 4),
            Show(EnemyFrameId.DeadSidehopperHopBody, 5),
            Show(EnemyFrameId.DeadSidehopperHopAlt, 4),
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
        [P.ZoomerCorpse0] =
        [
            Show(EnemyFrameId.DeadZoomerCorpseFirst, 1),
        ],
        [P.ZoomerCorpse1] =
        [
            Show(EnemyFrameId.DeadZoomerCorpseSecond, 1),
        ],
        [P.ZoomerCorpse2] =
        [
            Show(EnemyFrameId.DeadZoomerCorpseThird, 1),
        ],
        [P.RipperCorpse0] =
        [
            Show(EnemyFrameId.DeadRipperCorpseFirst, 1),
        ],
        [P.RipperCorpse1] =
        [
            Show(EnemyFrameId.DeadRipperCorpseSecond, 1),
        ],
        [P.SkreeCorpse0] =
        [
            Show(EnemyFrameId.DeadSkreeCorpseFirst, 1),
        ],
        [P.SkreeCorpse1] =
        [
            Show(EnemyFrameId.DeadSkreeCorpseSecond, 1),
        ],
        [P.SkreeCorpse2] =
        [
            Show(EnemyFrameId.DeadSkreeCorpseThird, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTourianSegment, Animation<DeadTourianSegment>> StartingAt { get; } = new Dictionary<DeadTourianSegment, Animation<DeadTourianSegment>>
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
    internal static Animation<DeadTourianSegment> StartingWith(DeadTourianSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Cue(DeadTourianCue cue) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Event(cue);
}
