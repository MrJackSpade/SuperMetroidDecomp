using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shapes of the tilemaps Kraid's body sheet is composed from (#1413, #1466): his two installed body tilemaps, the
/// working map of two 32-by-32 pages the cartridge builds from them, and the head images drawn over its upper page.
/// Where each source sits in the cartridge is <c>CartridgeTables/KraidSourceDefinitions</c>.
/// </summary>
internal static class KraidWorkingMapDefinitions
{
    /// <summary>The installed length of either body tilemap (upper or lower): 2048 two-byte cells.</summary>
    internal const int SourceTilemapBytes = 4096;

    /// <summary>The cells of either installed body tilemap (upper or lower).</summary>
    internal const int SourceTilemapCells = 2048;

    /// <summary>The cells of Kraid's working body map: the upper page followed by the lower page.</summary>
    internal const int WorkingMapCells = 2048;

    /// <summary>The first cell of the lower page in Kraid's working body map.</summary>
    internal const int LowerPageFirstCell = 1024;

    /// <summary>The cells of the lower body tilemap copied into the lower page: its first twenty-four rows.</summary>
    internal const int LowerSourceCopiedCells = 768;

    /// <summary>The cells of either visible 32-by-32 body page.</summary>
    internal const int PageCells = 1024;

    /// <summary>The cells of one head image: 32 columns by 11 rows.</summary>
    internal const int HeadImageCells = 352;

    /// <summary>The first cell of the working map's final row, which shows the blank tile.</summary>
    internal const int BlankRowFirstCell = 2016;

    /// <summary>The blank tile Kraid's working map's final row and every unshown page cell hold: tile 824 under colour variant zero, unmirrored, low priority.</summary>
    internal static BackgroundTile BlankTile { get; } = new(824, 0, highPriority: false);
}
