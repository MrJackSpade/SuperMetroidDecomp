using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Physical foreground at8C:BEC3. Regular regions are constructed geometrically;
/// the left silhouette is authored collision-mask data reviewed under1165.</summary>
internal static class IntroMotherBrainCollisionDefinitions
{
    /// <summary>Sixteen level blocks per physical source row.</summary>
    internal const int Columns = 16;
    /// <summary>Fourteen imported rows; the native copy is448 bytes.</summary>
    internal const int SourceRows = 14;
    /// <summary>A solid block with no visual tile.</summary>
    internal static ushort Solid => RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock).Raw;
    /// <summary>A slope block of shape zero with no visual tile: the platforms' top collision cells.</summary>
    internal static ushort PlatformTop => RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.Slope).Raw;
    /// <summary>First and last nonzero full-width source rows.</summary>
    internal const int CeilingRow = 1, FloorRow = 13;
    /// <summary>Upper central formation spans rows2/3,centered on column9.</summary>
    internal const int UpperTipRow = 2, CentralColumn = 9;
    /// <summary>Lower central and right platforms start at row8;right platform starts column12.</summary>
    internal const int PlatformRow = 8, RightPlatformColumn = 12;

    /// <summary>Authored collision-mask widths on rows2..12: tank interior,upper notch and pipe base.
    /// These select blocking geometry independently of visible pixels; a fitted curve would
    /// restate or alter the chosen level mask. See introMotherBrainCollisionContourDisposition.</summary>
    internal static ReadOnlySpan<byte> LeftContourWidths => [6, 6, 5, 6, 4, 4, 4, 6, 7, 7, 7];
}
