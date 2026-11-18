using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Artwork;
using C = SuperMetroid.Core.Rooms.RoomObjects.CollectibleSequence;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Everything a collectible shows at its block (#1994, #2480), played by the shared animation player. A Chozo orb's shell
/// shows dim for twenty updates, glowing for ten, bright for twenty and glowing again for ten, then starts again (#1825).
/// A shot Chozo orb shows the empty block, its burst and the empty block again, three updates each, before its item
/// appears. An exposed item alternates its two looks, four updates each, until Samus takes it. A shot block crumbles from
/// whole to dust, four updates each, uncovering its item, which alternates its looks for twenty-two pairs; then the block
/// forms again from dust over it, showing whole once more as its saved block returns. A collected item's shot block stands
/// empty for twenty-two eight-update intervals, then grows back from dust to rock, four updates each, before its saved
/// block returns. A picked-up item's empty block shows for one update before the item leaves.
/// </summary>
internal static class CollectibleSequenceAnimations
{
    /// <summary>Each run's images and how long each shows; a revealed item's last look marks the block starting to form again.</summary>
    internal static AnimationSegments<CollectibleSequence, CollectibleImage, CollectiblePhase> Segments { get; } = new(
        new Dictionary<CollectibleSequence, AnimationBeat<CollectibleImage, CollectiblePhase>[]>
        {
            [C.OrbPulsing] =
            [
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ChozoOrbDim), 20),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ChozoOrbGlowing), 10),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ChozoOrbBright), 20),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ChozoOrbGlowing), 10),
            ],
            [C.OrbBurst] =
            [
                Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.OrbBurstImageUpdates),
                Show(RoomObjectCollectibleDrawDefinitions.OrbBurst, CollectibleTimingDefinitions.OrbBurstImageUpdates),
                Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.OrbBurstImageUpdates),
            ],
            [C.Looks] = [Look(CollectibleLook.Primary), Look(CollectibleLook.Alternate)],
            [C.RevealedLastPrimary] = [Look(CollectibleLook.Primary)],
            [C.RevealedLastAlternate] =
            [
                new AnimationBeat<CollectibleImage, CollectiblePhase>.Marker(CollectiblePhase.ShotBlockReconceal),
                Look(CollectibleLook.Alternate),
            ],
            [C.ShotBlockReveal] =
            [
                Show(RoomObjectCollectibleDrawDefinitions.ShotBlockIntact, CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockCracking), CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockDust), CollectibleTimingDefinitions.RevealImageUpdates),
            ],
            [C.ShotBlockConceal] =
            [
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockDust), CollectibleTimingDefinitions.RevealImageUpdates),
                Show(CollectibleObjectDraws.Family.Of(CollectibleDraw.ShotBlockCracking), CollectibleTimingDefinitions.RevealImageUpdates),
            ],
            [C.CollectedShotBlockEmpty] = [Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.CollectedShotBlockEmptyUpdates)],
            [C.ShotBlockRespawn] =
            [
                Show(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Dust), CollectibleTimingDefinitions.RespawnImageUpdates),
                Show(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Pieces), CollectibleTimingDefinitions.RespawnImageUpdates),
                Show(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Rock), CollectibleTimingDefinitions.RespawnImageUpdates),
            ],
            [C.PickedUpEmpty] = [Show(RoomObjectCollectibleDrawDefinitions.Empty, CollectibleTimingDefinitions.PickedUpEmptyUpdates)],
        });

    /// <summary>A Chozo orb's pulse, again and again until a shot breaks the orb.</summary>
    internal static Animation<CollectibleSequence> OrbPulsing { get; } = Animation<CollectibleSequence>.Looping([Segments.Whole(C.OrbPulsing)]);

    /// <summary>A shot Chozo orb bursting open.</summary>
    internal static Animation<CollectibleSequence> OrbBurst { get; } = Once(C.OrbBurst);

    /// <summary>An exposed item's looks, again and again until Samus takes it.</summary>
    internal static Animation<CollectibleSequence> Exposed { get; } = Animation<CollectibleSequence>.Looping([Segments.Whole(C.Looks)]);

    /// <summary>
    /// A revealed item's twenty-two pairs of looks, then its shot block forming again over it. The block starts to form as
    /// the last pair's alternate look shows, which the marker before it raises, and the item is hidden again once the
    /// animation is over.
    /// </summary>
    internal static Animation<CollectibleSequence> Revealed { get; } = new(
        [
            Segments.Repeated(C.Looks, CollectibleTimingDefinitions.RevealedImagePairs - 1),
            Segments.Whole(C.RevealedLastPrimary),
            Segments.Whole(C.RevealedLastAlternate),
            Segments.Whole(C.ShotBlockConceal),
        ],
        AnimationEnd.Stops);

    /// <summary>A shot block crumbling away from its item.</summary>
    internal static Animation<CollectibleSequence> ShotBlockReveal { get; } = Once(C.ShotBlockReveal);

    /// <summary>A collected item's shot block standing empty.</summary>
    internal static Animation<CollectibleSequence> CollectedShotBlockEmpty { get; } = Once(C.CollectedShotBlockEmpty);

    /// <summary>A collected item's shot block growing back.</summary>
    internal static Animation<CollectibleSequence> ShotBlockRespawn { get; } = Once(C.ShotBlockRespawn);

    /// <summary>A picked-up item's empty block.</summary>
    internal static Animation<CollectibleSequence> PickedUpEmpty { get; } = Once(C.PickedUpEmpty);

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    private static Animation<CollectibleSequence> Once(CollectibleSequence segment) =>
        new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Shows <paramref name="draw"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CollectibleImage, CollectiblePhase> Show(RoomObjectDraw draw, int duration) =>
        new AnimationBeat<CollectibleImage, CollectiblePhase>.Frame(CollectibleImage.Drawn(draw), duration);

    /// <summary>Shows the item's own <paramref name="look"/> for its visible image's updates.</summary>
    private static AnimationBeat<CollectibleImage, CollectiblePhase> Look(CollectibleLook look) =>
        new AnimationBeat<CollectibleImage, CollectiblePhase>.Frame(CollectibleImage.Visible(look), CollectibleTimingDefinitions.VisibleImageUpdates);
}
