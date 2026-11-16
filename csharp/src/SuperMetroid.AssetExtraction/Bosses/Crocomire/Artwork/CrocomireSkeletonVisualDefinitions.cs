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
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By134,
        EnemyFrameId.CrocomireBodySkeletonCollapsing141By132,
        EnemyFrameId.CrocomireBodySkeletonCollapsing142By127,
        EnemyFrameId.CrocomireBodySkeletonCollapsing143By130,
        EnemyFrameId.CrocomireBodySkeletonCollapsing143By131,
        EnemyFrameId.CrocomireBodySkeletonCollapsing160By130,
        EnemyFrameId.CrocomireBodySkeletonCollapsing159By122,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By118,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By108,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By109,
        EnemyFrameId.CrocomireBodySkeletonCollapsing177By87,
        EnemyFrameId.CrocomireBodySkeletonCollapsing174By82,
        EnemyFrameId.CrocomireBodySkeletonCollapsing185By79,
        EnemyFrameId.CrocomireBodySkeletonCollapsing189By77,
        EnemyFrameId.CrocomireBodySkeletonCollapsing173By77,
        EnemyFrameId.CrocomireBodySkeletonCollapsing153By84,
        EnemyFrameId.CrocomireBodySkeletonCollapsing120By71,
        EnemyFrameId.CrocomireBodySkeletonCollapsing120By68,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubbles6By7Risen0,
        EnemyFrameId.CrocomireBubbles12By12Risen6,
        EnemyFrameId.CrocomireBubbles14By13Risen8,
        EnemyFrameId.CrocomireBubbles12By14Risen11,
        EnemyFrameId.CrocomireBubbles12By14Risen14,
        EnemyFrameId.CrocomireBubbles10By13Risen15,
        EnemyFrameId.CrocomireBubbles10By12Risen16,
        EnemyFrameId.CrocomireBubbles8By10Risen16,
        EnemyFrameId.CrocomireBubbles4By6Risen14,
        EnemyFrameId.CrocomireBubbles4By6Adrift,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
