using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;

/// <summary>Sprite composition roots for Crocomire's falling, collapsing, stable and river skeleton.</summary>
internal static class CrocomireSkeletonVisualDefinitions
{
    /// <summary>Thirty-three corpse roots selected by the compiled programs.</summary>
    internal const int FrameCount = 33;
    /// <summary>The collapse poses contain thirteen sprite components, not eight.</summary>
    internal const int MaximumComponents = 13;


    /// <summary>The thirty-three skeleton frames the compiled programs select.</summary>
    internal static ImmutableArray<EnemyFrameId> Frames { get; } =
    [
        EnemyFrameId.CrocomireBodySkeletonFallingFirst,
        EnemyFrameId.CrocomireBodySkeletonFallingSecond,
        EnemyFrameId.CrocomireBodySkeletonFallingThird,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFirst,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSecond,
        EnemyFrameId.CrocomireBodySkeletonFallsApartThird,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFourth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFifth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSixth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSeventh,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEighth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartNinth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartTenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEleventh,
        EnemyFrameId.CrocomireBodySkeletonFallsApartTwelfth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartThirteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFourteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFifteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSixteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSeventeenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEighteenth,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFirst,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSecond,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverThird,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFourth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFifth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSixth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSeventh,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverEighth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverNinth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverTenth,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
