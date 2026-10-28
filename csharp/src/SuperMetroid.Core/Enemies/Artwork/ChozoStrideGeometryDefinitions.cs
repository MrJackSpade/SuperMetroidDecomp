using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Applies the four native Chozo stride foot positions to matching supplied spritemaps
/// while preserving the appearance and order of their other artwork parts.
/// </summary>
internal static class ChozoStrideGeometryDefinitions
{
    /// <summary>The eight stride compositions, Spritemaps_Chozo_4 onwards: two strides of four phases.</summary>
    internal static ImmutableArray<EnemyFrameId> Poses { get; } =
    [
        EnemyFrameId.ChozoStatuePose4, EnemyFrameId.ChozoStatuePose5, EnemyFrameId.ChozoStatuePose6, EnemyFrameId.ChozoStatuePose7,
        EnemyFrameId.ChozoStatuePose8, EnemyFrameId.ChozoStatuePose9, EnemyFrameId.ChozoStatuePose10, EnemyFrameId.ChozoStatuePose11,
    ];
    /// <summary>Width of one OBJ glyph, the distance from the left foot to the adjacent right foot.</summary>
    internal const int TilePixels = 8;

    /// <summary>One half of the support foot: its OBJ character and its distance from the left half's origin.</summary>
    /// <param name="Tile">The OBJ character drawing this half.</param>
    /// <param name="OffsetFromLeft">The half's horizontal distance from the left half's origin.</param>
    internal sealed record FootHalf(int Tile, int OffsetFromLeft);

    /// <summary>$AA:E954/E959 and following poses: the support foot's left half.</summary>
    internal static readonly FootHalf LeftHalf = new(0x170, 0);
    /// <summary>The support foot's right half, one glyph to the right of the left half.</summary>
    internal static readonly FootHalf RightHalf = new(0x171, TilePixels);
    /// <summary>$AA:E959/E9C4/EA34/EAA4: chosen left-foot origins -31/-28/-14/-7.</summary>
    private const int WeightShiftFootX = -31, EarlySupportFootX = -28,
        LateSupportFootX = -14, PushOffFootX = -7;

    /// <summary>The left support-foot origin for each of the four stride phases.</summary>
    internal static readonly int[] SupportFootXs = [WeightShiftFootX, EarlySupportFootX, LateSupportFootX, PushOffFootX];
}
