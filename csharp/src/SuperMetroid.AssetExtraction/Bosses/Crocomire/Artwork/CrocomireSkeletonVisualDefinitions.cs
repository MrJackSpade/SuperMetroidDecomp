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
        EnemyFrameId.CrocomireBodySkeletonArmHeldLevel,
        EnemyFrameId.CrocomireBodySkeletonArmDropping,
        EnemyFrameId.CrocomireBodySkeletonArmLowered,
        EnemyFrameId.CrocomireBodySkeletonArmPlanted,
        EnemyFrameId.CrocomireBodySkeletonBodySettling,
        EnemyFrameId.CrocomireBodySkeletonArmSplaying,
        EnemyFrameId.CrocomireBodySkeletonArmFlat,
        EnemyFrameId.CrocomireBodySkeletonRibsSagging,
        EnemyFrameId.CrocomireBodySkeletonRibsSlumped,
        EnemyFrameId.CrocomireBodySkeletonArmFallenOff,
        EnemyFrameId.CrocomireBodySkeletonTailScattering,
        EnemyFrameId.CrocomireBodySkeletonSpineFallingApart,
        EnemyFrameId.CrocomireBodySkeletonRibsSplaying,
        EnemyFrameId.CrocomireBodySkeletonRibsFlattened,
        EnemyFrameId.CrocomireBodySkeletonBonesGathering,
        EnemyFrameId.CrocomireBodySkeletonBonesHuddled,
        EnemyFrameId.CrocomireBodySkeletonRibsBesideSkull,
        EnemyFrameId.CrocomireBodySkeletonLastBones,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubblesForming,
        EnemyFrameId.CrocomireBubblesWelling,
        EnemyFrameId.CrocomireBubblesSpreading,
        EnemyFrameId.CrocomireBubblesBursting,
        EnemyFrameId.CrocomireBubblesSpraying,
        EnemyFrameId.CrocomireBubblesScattered,
        EnemyFrameId.CrocomireBubblesThinning,
        EnemyFrameId.CrocomireBubblesFading,
        EnemyFrameId.CrocomireBubblesVanishing,
        EnemyFrameId.CrocomireBubblesAdrift,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
