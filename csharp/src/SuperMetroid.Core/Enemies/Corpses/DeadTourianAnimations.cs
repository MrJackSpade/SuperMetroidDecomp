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
            Show(EnemyFrameId.LargeSidehopperLegsBent, 2),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 4),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 5),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 48),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 5),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 4),
            Show(EnemyFrameId.LargeSidehopperLegsBent, 5),
            Show(EnemyFrameId.LargeSidehopperLegsCrouched, 4),
            Cue(new(K.EndHop)),
        ],
        [P.SidehopperIdle] = [Hold(EnemyFrameId.LargeSidehopperLegsStraight)],
        [P.SidehopperCorpse] = [Hold(EnemyFrameId.DeadSidehopperSprawled)],
        [P.SidehopperInitiallyDead] = [Hold(EnemyFrameId.DeadSidehopperSprawledLegsUp)],
        [P.ZoomerCorpseOnItsBack] = [Hold(EnemyFrameId.DeadZoomerCorpseOnItsBack)],
        [P.ZoomerCorpseUpright] = [Hold(EnemyFrameId.DeadZoomerCorpseUpright)],
        [P.RipperCorpseLevel] = [Hold(EnemyFrameId.DeadRipperCorpseLevel)],
        [P.RipperCorpseTilted] = [Hold(EnemyFrameId.DeadRipperCorpseTilted)],
        [P.SkreeCorpse] = [Hold(EnemyFrameId.DeadSkreeCorpse)],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<DeadTourianSegment, Animation<DeadTourianSegment>> StartingAt { get; } = Segments.EachEnding(AnimationEnd.Holds,
        P.SidehopperHopping, P.SidehopperIdle, P.SidehopperCorpse, P.SidehopperInitiallyDead, P.ZoomerCorpseOnItsBack,
        P.ZoomerCorpseUpright, P.RipperCorpseLevel, P.RipperCorpseTilted, P.SkreeCorpse);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTourianSegment> StartingWith(DeadTourianSegment segment) => StartingAt[segment];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, DeadTourianCue> Cue(DeadTourianCue cue) => new AnimationBeat<EnemyFrameId, DeadTourianCue>.Event(cue);
}
