using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The Chozo stride poses whose support-foot glyphs the import pipeline places at the stride's planted-foot positions,
/// and the glyphs it places, while preserving the appearance and order of the poses' other artwork parts.
/// </summary>
internal static class ChozoStrideGeometryDefinitions
{
    /// <summary>The eight stride compositions, Spritemaps_Chozo_4 onwards: two strides of four phases.</summary>
    internal static ImmutableArray<EnemyFrameId> Poses { get; } =
    [
        EnemyFrameId.ChozoStatuePose4, EnemyFrameId.ChozoStatuePose5, EnemyFrameId.ChozoStatuePose6, EnemyFrameId.ChozoStatuePose7,
        EnemyFrameId.ChozoStatuePose8, EnemyFrameId.ChozoStatuePose9, EnemyFrameId.ChozoStatuePose10, EnemyFrameId.ChozoStatuePose11,
    ];
    /// <summary>The stride phases in one stride, the length of the support-foot cycle.</summary>
    internal const int PhasesPerStride = 4;
    /// <summary>Width of one OBJ glyph, the distance from the left foot to the adjacent right foot.</summary>
    internal const int TilePixels = 8;

    /// <summary>One half of the support foot: its OBJ character and its distance from the left half's origin.</summary>
    /// <param name="Tile">The OBJ character drawing this half.</param>
    /// <param name="OffsetFromLeft">The half's horizontal distance from the left half's origin.</param>
    internal sealed record FootHalf(int Tile, int OffsetFromLeft);

    /// <summary>$AA:E954/E959 and following poses: the support foot's left half.</summary>
    internal static readonly FootHalf LeftHalf = new(368, 0);
    /// <summary>The support foot's right half, one glyph to the right of the left half.</summary>
    internal static readonly FootHalf RightHalf = new(369, TilePixels);
}
