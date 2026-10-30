using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Physical foreground of the intro Mother Brain room. Regular regions are constructed geometrically;
/// the left silhouette is authored collision-mask data reviewed under 1165.</summary>
internal static class IntroMotherBrainCollisionDefinitions
{
    /// <summary>Sixteen blocks per room row.</summary>
    internal const int Columns = 16;
    /// <summary>Fourteen authored rows.</summary>
    internal const int SourceRows = 14;
    /// <summary>The room's height in blocks; the rows past the authored ones are empty.</summary>
    internal const int RoomRows = 16;
    /// <summary>A solid block showing block zero.</summary>
    internal static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    /// <summary>A slope block showing block zero: the platforms' top collision cells.</summary>
    internal static readonly RoomTerrainBlock PlatformTop = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Slope);
    /// <summary>An air block showing block zero: every cell the authored collision leaves open.</summary>
    internal static readonly RoomTerrainBlock Empty = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);
    /// <summary>First and last nonzero full-width source rows.</summary>
    internal const int CeilingRow = 1, FloorRow = 13;
    /// <summary>Upper central formation spans rows 2/3, centered on column 9.</summary>
    internal const int UpperTipRow = 2, CentralColumn = 9;
    /// <summary>Lower central and right platforms start at row 8; right platform starts column 12.</summary>
    internal const int PlatformRow = 8, RightPlatformColumn = 12;

    /// <summary>Authored collision-mask widths on rows 2..12: tank interior, upper notch and pipe base.
    /// These select blocking geometry independently of visible pixels; a fitted curve would
    /// restate or alter the chosen level mask. See introMotherBrainCollisionContourDisposition.</summary>
    internal static ReadOnlySpan<byte> LeftContourWidths => [6, 6, 5, 6, 4, 4, 4, 6, 7, 7, 7];
}
