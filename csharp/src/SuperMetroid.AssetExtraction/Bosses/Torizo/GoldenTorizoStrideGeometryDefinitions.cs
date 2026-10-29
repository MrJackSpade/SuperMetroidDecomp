using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// The Golden Torizo walking frames whose foot strips the import pipeline places at the stride's planted-foot positions,
/// and the strips it places. All independent artwork remains required under the installed extended frames; none is
/// exempted as a motion-table conversion.
/// </summary>
internal static class GoldenTorizoStrideGeometryDefinitions
{
    /// <summary>The ten four-component left-walking extended frames, one per walking phase.</summary>
    internal static ImmutableArray<EnemyFrameId> WalkingFrames { get; } =
    [
        EnemyFrameId.GoldenTorizoWalkLeft0, EnemyFrameId.GoldenTorizoWalkLeft1, EnemyFrameId.GoldenTorizoWalkLeft2,
        EnemyFrameId.GoldenTorizoWalkLeft3, EnemyFrameId.GoldenTorizoWalkLeft4, EnemyFrameId.GoldenTorizoWalkLeft5,
        EnemyFrameId.GoldenTorizoWalkLeft6, EnemyFrameId.GoldenTorizoWalkLeft7, EnemyFrameId.GoldenTorizoWalkLeft8,
        EnemyFrameId.GoldenTorizoWalkLeft9,
    ];
    /// <summary>The walking frames' body component, which carries the planted foot.</summary>
    internal const int BodyComponent = 2;
    /// <summary>$AA:8FD6-8FE4 and the other body poses: the first of the three 8px floor-strip tiles $160/$161/$162.</summary>
    internal const int FirstFootTile = 0x160;
    /// <summary>The floor-strip tiles in one foot strip.</summary>
    internal const int FootTiles = 3;
    /// <summary>Width of one floor-strip tile.</summary>
    internal const int TilePixels = 8;
    /// <summary>The leg whose foot strip draws in each sprite palette row; a strip in any other row is not a foot.</summary>
    internal static IReadOnlyDictionary<int, GoldenTorizoLeg> LegsByPalette { get; } = new Dictionary<int, GoldenTorizoLeg>
    {
        [1] = GoldenTorizoLeg.First,
        [2] = GoldenTorizoLeg.Second,
    };
}
