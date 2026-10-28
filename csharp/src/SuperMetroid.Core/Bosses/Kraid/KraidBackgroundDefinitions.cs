using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The shapes of the backgrounds Kraid draws (#1413): his body map, built from two installed body tilemaps into a
/// working map of two 32-by-32 pages, the head images drawn over its upper page, and the room backdrop's characters.
/// Where each source sits in the cartridge is the import's knowledge (<c>AssetExtraction/CartridgeTables/KraidSourceDefinitions</c>).
/// </summary>
public static class KraidBackgroundDefinitions
{
    /// <summary>The installed length of either body tilemap (upper or lower): 2048 two-byte cells.</summary>
    public const int SourceTilemapBytes = 4096;

    /// <summary>The cells of either installed body tilemap (upper or lower).</summary>
    public const int SourceTilemapCells = 2048;

    /// <summary>The cells of Kraid's working body map: the upper page followed by the lower page.</summary>
    public const int WorkingMapCells = 2048;

    /// <summary>The first cell of the lower page in Kraid's working body map.</summary>
    public const int LowerPageFirstCell = 1024;

    /// <summary>The cells of the lower body tilemap copied into the lower page: its first twenty-four rows.</summary>
    public const int LowerSourceCopiedCells = 768;

    /// <summary>The cells of either visible 32-by-32 body page.</summary>
    public const int PageCells = 1024;

    /// <summary>The cells of one head image: 32 columns by 11 rows.</summary>
    public const int HeadImageCells = 352;

    /// <summary>The first cell of the working map's final row, which shows the blank tile.</summary>
    public const int BlankRowFirstCell = 2016;

    /// <summary>The blank tile Kraid's working map's final row and every unshown page cell hold: tile 824 under colour variant zero, unmirrored, low priority.</summary>
    public static BackgroundTile BlankTile { get; } = new(824, 0, highPriority: false);

    /// <summary>The first of the room's tile characters the room backdrop binds over.</summary>
    public const int RoomBackgroundFirstCharacter = 1008;

    /// <summary>The size of the background plane Kraid's body shows on: two screens wide and two screens tall.</summary>
    public static Extent16<TilemapCells> BodyPlaneSize { get; } = new(64, 64);
}
