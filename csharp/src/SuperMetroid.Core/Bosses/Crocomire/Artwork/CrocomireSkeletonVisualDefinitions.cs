using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>OAM composition roots for Crocomire's falling, collapsing, stable and river skeleton.</summary>
internal static class CrocomireSkeletonVisualDefinitions
{
    /// <summary>Thirty-three corpse roots selected by the compiled bank-$A4 programs.</summary>
    internal const int FrameCount = 33;
    /// <summary>The collapse poses at $A4:E46A..E53E contain thirteen OAM components, not eight.</summary>
    internal const int MaximumComponents = 13;


    /// <summary>The thirty-three skeleton frames the compiled programs select.</summary>
    internal static ImmutableArray<EnemyFrameId> Frames { get; } =
    [
        EnemyFrameId.CrocomireSkeletonOamE1FE,
        EnemyFrameId.CrocomireSkeletonOamE228,
        EnemyFrameId.CrocomireSkeletonOamE252,
        EnemyFrameId.CrocomireSkeletonOamE27C,
        EnemyFrameId.CrocomireSkeletonOamE2A6,
        EnemyFrameId.CrocomireSkeletonOamE2D0,
        EnemyFrameId.CrocomireSkeletonOamE2FA,
        EnemyFrameId.CrocomireSkeletonOamE324,
        EnemyFrameId.CrocomireSkeletonOamE34E,
        EnemyFrameId.CrocomireSkeletonOamE378,
        EnemyFrameId.CrocomireSkeletonOamE3A2,
        EnemyFrameId.CrocomireSkeletonOamE3CC,
        EnemyFrameId.CrocomireSkeletonOamE3F6,
        EnemyFrameId.CrocomireSkeletonOamE420,
        EnemyFrameId.CrocomireSkeletonOamE46A,
        EnemyFrameId.CrocomireSkeletonOamE4D4,
        EnemyFrameId.CrocomireSkeletonOamE53E,
        EnemyFrameId.CrocomireSkeletonOamE5A8,
        EnemyFrameId.CrocomireSkeletonOamE60A,
        EnemyFrameId.CrocomireSkeletonOamE65C,
        EnemyFrameId.CrocomireSkeletonOamE68E,
        EnemyFrameId.CrocomireSkeletonOamE6A8,
        EnemyFrameId.CrocomireSkeletonOamE6B2,
        EnemyFrameId.CrocomireSkeletonOamE6BC,
        EnemyFrameId.CrocomireSkeletonOamE6C6,
        EnemyFrameId.CrocomireSkeletonOamE6D0,
        EnemyFrameId.CrocomireSkeletonOamE6DA,
        EnemyFrameId.CrocomireSkeletonOamE6E4,
        EnemyFrameId.CrocomireSkeletonOamE6EE,
        EnemyFrameId.CrocomireSkeletonOamE6F8,
        EnemyFrameId.CrocomireSkeletonOamE702,
        EnemyFrameId.CrocomireSkeletonOamE70C,
        EnemyFrameId.CrocomireSkeletonOamE716,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
