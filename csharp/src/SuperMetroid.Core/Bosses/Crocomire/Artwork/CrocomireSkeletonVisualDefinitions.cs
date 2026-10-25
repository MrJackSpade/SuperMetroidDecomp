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
        EnemyFrameId.CrocomireSkeleton0,
        EnemyFrameId.CrocomireSkeleton1,
        EnemyFrameId.CrocomireSkeleton2,
        EnemyFrameId.CrocomireSkeleton3,
        EnemyFrameId.CrocomireSkeleton4,
        EnemyFrameId.CrocomireSkeleton5,
        EnemyFrameId.CrocomireSkeleton6,
        EnemyFrameId.CrocomireSkeleton7,
        EnemyFrameId.CrocomireSkeleton8,
        EnemyFrameId.CrocomireSkeleton9,
        EnemyFrameId.CrocomireSkeleton10,
        EnemyFrameId.CrocomireSkeleton11,
        EnemyFrameId.CrocomireSkeleton12,
        EnemyFrameId.CrocomireSkeleton13,
        EnemyFrameId.CrocomireSkeleton14,
        EnemyFrameId.CrocomireSkeleton15,
        EnemyFrameId.CrocomireSkeleton16,
        EnemyFrameId.CrocomireSkeleton17,
        EnemyFrameId.CrocomireSkeleton18,
        EnemyFrameId.CrocomireSkeleton19,
        EnemyFrameId.CrocomireSkeleton20,
        EnemyFrameId.CrocomireSkeleton21,
        EnemyFrameId.CrocomireSkeleton22,
        EnemyFrameId.CrocomireSkeleton23,
        EnemyFrameId.CrocomireSkeleton24,
        EnemyFrameId.CrocomireSkeleton25,
        EnemyFrameId.CrocomireSkeleton26,
        EnemyFrameId.CrocomireSkeleton27,
        EnemyFrameId.CrocomireSkeleton28,
        EnemyFrameId.CrocomireSkeleton29,
        EnemyFrameId.CrocomireSkeleton30,
        EnemyFrameId.CrocomireSkeleton31,
        EnemyFrameId.CrocomireSkeleton32,
    ];

    /// <summary>Whether <paramref name="frame"/> is one of the skeleton frames.</summary>
    internal static bool IsFrame(EnemyFrameId frame) => Frames.Contains(frame);
}
