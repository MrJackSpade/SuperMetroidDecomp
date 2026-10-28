using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// Canonical planted-foot geometry shared by Golden Torizo artwork and walking motion.
/// The seven selected origins and all independent artwork remain required under Stream3's
/// EnemyExtendedFrameCatalog frames; none is exempted as a motion-table conversion.
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
    /// <summary>$AA:968D/8FE0/9720/91B4: the incoming support foot begins at X=-37.</summary>
    internal const int ForwardPlantX = -37;
    /// <summary>$AA:905F/9233: early planted-foot origins for the two alternating legs.</summary>
    internal const int FirstEarlySupportX = -32, SecondEarlySupportX = -30;
    /// <summary>$AA:90CA/929E: both legs pass the same middle support origin X=-13.</summary>
    internal const int MiddleSupportX = -13;
    /// <summary>$AA:9135/930E: final single-foot support origins X=3/5.</summary>
    internal const int FirstPushOffX = 3, SecondPushOffX = 5;
    /// <summary>$AA:967E/96F8: double-plant poses put the outgoing foot at X=10.</summary>
    internal const int DepartingFootX = 10;
    /// <summary>$AA:8FD6-8FE4 and the other body poses: three8px floor-strip tiles $160/$161/$162.</summary>
    internal const int FirstFootTile = 0x160, FootTiles = 3, TilePixels = 8;

    /// <summary>
    /// The planted foot's local X origin by walking phase. Each five-pose half plants the incoming foot for two poses,
    /// then passes its early, middle and push-off origins; only the early and push-off origins differ between the halves.
    /// </summary>
    internal static readonly int[] SupportFootXs =
    [
        ForwardPlantX, ForwardPlantX, FirstEarlySupportX, MiddleSupportX, FirstPushOffX,
        ForwardPlantX, ForwardPlantX, SecondEarlySupportX, MiddleSupportX, SecondPushOffX,
    ];
}
