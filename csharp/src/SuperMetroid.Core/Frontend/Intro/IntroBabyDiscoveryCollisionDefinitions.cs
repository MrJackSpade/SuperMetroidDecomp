using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Immutable physical level copied by the SR388 discovery setup. This 32-by-12
/// source is distinct from the visible BG page; the final four 32-block rows of
/// the declared room retain their native zero initialization.
/// </summary>
internal static class IntroBabyDiscoveryCollisionDefinitions
{
    /// <summary>Thirty-two physical foreground words per source row.</summary>
    internal const int Columns = 32;
    /// <summary>Twelve ROM-authored rows; the room itself is sixteen rows tall.</summary>
    internal const int SourceRows = 12;
    /// <summary>Ten zero rows precede the two authored nonzero rows.</summary>
    internal const int ZeroRows = 10;
    /// <summary>The eleventh source row's block in every column: a slope block of shape zero with no visual tile.</summary>
    internal static ushort PenultimateRowWord => RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.Slope).Raw;
    /// <summary>The final source row's block in every column: a solid block with no visual tile.</summary>
    internal static ushort FinalRowWord => RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock).Raw;

    /// <summary>Native room height, including four zero-initialized rows beyond the imported region.</summary>
    internal const int RoomRows = 16;
}