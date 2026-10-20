using System.Collections.Immutable;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Named presentation sources for the fixed Ceres escape transfer metadata.</summary>
internal static class CeresEscapeTileArtworkDefinitions
{
    /// <summary>Escape timer warning-text characters at $B7:DA00-E2FF.</summary>
    internal static readonly CeresEscapeTileSheetDefinition WarningText =
        new(CeresEscapeTileRomData.WarningTextSource,
            CeresEscapeTileRomData.WarningTextByteCount, "ceres-escape-warning-tiles.png");

    /// <summary>Ceres escape door characters at $B0:BA00-BFFF.</summary>
    internal static readonly CeresEscapeTileSheetDefinition Doors =
        new(CeresEscapeTileRomData.DoorSource,
            CeresEscapeTileRomData.DoorByteCount, "ceres-escape-door-tiles.png");

    /// <summary>The two fixed Ceres escape tile pages in native transfer order: the warning text, then the doors.</summary>
    internal static ImmutableArray<CeresEscapeTileSheetDefinition> All { get; } = [WarningText, Doors];

    /// <summary>Accepts only a positive byte range wholly inside one of the two Ceres escape DMA pages.</summary>
    internal static bool Contains(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeTileSheetDefinition page in All)
        {
            int offset = sourceAddress - page.SourceAddress;
            if (offset >= 0 && byteCount > 0 && offset <= page.ByteCount - byteCount)
                return true;
        }
        return false;
    }
}
