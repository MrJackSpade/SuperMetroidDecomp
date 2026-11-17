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
            Show(EnemyFrameId.DeadSidehopperLegsBent, 2),
            Show(EnemyFrameId.DeadSidehopperLegsCrouched, 4),
            Show(EnemyFrameId.DeadSidehopperLegsBent, 5),
            Show(EnemyFrameId.DeadSidehopperLegsCrouched, 48),
            Show(EnemyFrameId.DeadSidehopperLegsBent, 5),
            Show(EnemyFrameId.DeadSidehopperLegsCrouched, 4),
            Show(EnemyFrameId.DeadSidehopperLegsBent, 5),
            Show(EnemyFrameId.DeadSidehopperLegsCrouched, 4),
            Cue(new(K.EndHop)),
        ],
        [P.SidehopperIdle] = [Hold(EnemyFrameId.DeadSidehopperLegsStraight)],
        [P.SidehopperCorpse] = [Hold(EnemyFrameId.DeadSidehopperSprawled)],
        [P.SidehopperInitiallyDead] = [Hold(EnemyFrameId.DeadSidehopperSprawledLegsUp)],
        [P.ZoomerCorpseOnItsBack] = [Hold(EnemyFrameId.DeadZoomerCorpseOnItsBack)],
        [P.ZoomerCorpseUpright] = [Hold(EnemyFrameId.DeadZoomerCorpseUpright)],
        [P.RipperCorpseLevel] = [Hold(EnemyFrameId.DeadRipperCorpseLevel)],
        [P.RipperCorpseTilted] = [Hold(EnemyFrameId.DeadRipperCorpseTilted)],
        [P.SkreeCorpse] = [Hold(EnemyFrameId.DeadSkreeCorpse)],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DeadTourianSegment, Animation<DeadTourianSegment>> StartingAt { get; } = new Dictionary<DeadTourianSegment, Animation<DeadTourianSegment>>
    {
        [P.SidehopperHopping] = new([Whole(P.SidehopperHopping)], AnimationEnd.Holds),
        [P.SidehopperIdle] = new([Whole(P.SidehopperIdle)], AnimationEnd.Holds),
        [P.SidehopperCorpse] = new([Whole(P.SidehopperCorpse)], AnimationEnd.Holds),
        [P.SidehopperInitiallyDead] = new([Whole(P.SidehopperInitiallyDead)], AnimationEnd.Holds),
        [P.ZoomerCorpseOnItsBack] = new([Whole(P.ZoomerCorpseOnItsBack)], AnimationEnd.Holds),
        [P.ZoomerCorpseUpright] = new([Whole(P.ZoomerCorpseUpright)], AnimationEnd.Holds),
        [P.RipperCorpseLevel] = new([Whole(P.RipperCorpseLevel)], AnimationEnd.Holds),
        [P.RipperCorpseTilted] = new([Whole(P.RipperCorpseTilted)], AnimationEnd.Holds),
        [P.SkreeCorpse] = new([Whole(P.SkreeCorpse)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTourianSegment> StartingWith(DeadTourianSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<DeadTourianSegment> Whole(DeadTourianSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Cue(DeadTourianCue cue) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Event(cue);
}
