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
        EnemyFrameId.CrocomireBodySkeletonToppling,
        EnemyFrameId.CrocomireBodySkeletonLeaning,
        EnemyFrameId.CrocomireBodySkeletonTipping,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage1,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage2,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage3,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage4,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage5,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage6,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage7,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage8,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage9,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage10,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage11,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage12,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage13,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage14,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage15,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage16,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage17,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage18,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubblesRisingStage1,
        EnemyFrameId.CrocomireBubblesRisingStage2,
        EnemyFrameId.CrocomireBubblesRisingStage3,
        EnemyFrameId.CrocomireBubblesRisingStage4,
        EnemyFrameId.CrocomireBubblesRisingStage5,
        EnemyFrameId.CrocomireBubblesRisingStage6,
        EnemyFrameId.CrocomireBubblesRisingStage7,
        EnemyFrameId.CrocomireBubblesRisingStage8,
        EnemyFrameId.CrocomireBubblesRisingStage9,
        EnemyFrameId.CrocomireBubblesAdrift,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
