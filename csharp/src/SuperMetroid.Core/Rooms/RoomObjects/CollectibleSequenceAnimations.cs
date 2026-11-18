using SuperMetroid.Core.Animation;
using Shape = SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions.Shape;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The short animations a collectible plays at its block (#1994), played by the shared animation playback and over once
/// their last image's time is up. A shot Chozo orb shows the empty block, its burst and the empty block again, three updates
/// each, before its item appears. A shot block crumbles from whole to dust, four updates each, uncovering its item; it
/// forms again from dust over an item nobody took, showing whole once more as its saved block returns. A collected item's
/// shot block grows back from dust to rock, four updates each, before its saved block returns.
/// </summary>
internal static class CollectibleSequenceAnimations
{
    /// <summary>Each animation's images and how long each shows.</summary>
    internal static AnimationSegments<CollectibleSequence, CollectibleImage, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CollectibleSequence, AnimationBeat<CollectibleImage, NoAnimationCue>[]>
        {
            [CollectibleSequence.OrbBurst] =
            [
                Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.OrbBurstImageUpdates),
                Show(RoomObjectCollectibleDrawDefinitions.OrbBurst, CollectibleTimingDefinitions.OrbBurstImageUpdates),
                Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.OrbBurstImageUpdates),
            ],
            [CollectibleSequence.ShotBlockReveal] =
            [
                Show(RoomObjectCollectibleDrawDefinitions.ShotBlockIntact, CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockCracking), CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockDust), CollectibleTimingDefinitions.RevealImageUpdates),
            ],
            [CollectibleSequence.ShotBlockConceal] =
            [
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockDust), CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockCracking), CollectibleTimingDefinitions.RevealImageUpdates),
            ],
            [CollectibleSequence.ShotBlockRespawn] =
            [
                Show(ShotBlockObjectDraws.Breakup(Shape.Single, BlockBreakupStage.Dust), CollectibleTimingDefinitions.RespawnImageUpdates),
                Show(ShotBlockObjectDraws.Breakup(Shape.Single, BlockBreakupStage.Pieces), CollectibleTimingDefinitions.RespawnImageUpdates),
                Show(ShotBlockObjectDraws.Breakup(Shape.Single, BlockBreakupStage.Rock), CollectibleTimingDefinitions.RespawnImageUpdates),
            ],
        });

    /// <summary>Each animation, played once through.</summary>
    internal static IReadOnlyDictionary<CollectibleSequence, Animation<CollectibleSequence>> Animations { get; } =
        new Dictionary<CollectibleSequence, Animation<CollectibleSequence>>
        {
            [CollectibleSequence.OrbBurst] = Once(CollectibleSequence.OrbBurst),
            [CollectibleSequence.ShotBlockReveal] = Once(CollectibleSequence.ShotBlockReveal),
            [CollectibleSequence.ShotBlockConceal] = Once(CollectibleSequence.ShotBlockConceal),
            [CollectibleSequence.ShotBlockRespawn] = Once(CollectibleSequence.ShotBlockRespawn),
        };

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    private static Animation<CollectibleSequence> Once(CollectibleSequence segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Shows <paramref name="draw"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CollectibleImage, NoAnimationCue> Show(RoomObjectDraw draw, int duration) =>
        new AnimationBeat<CollectibleImage, NoAnimationCue>.Frame(new CollectibleImage(draw), duration);
}
