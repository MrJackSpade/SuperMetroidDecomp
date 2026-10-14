namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Native atlas anchors for the planet's rims, two-row bands, and edge patches.</summary>
internal static class ZebesPlanetBandAtlas
{
    /// <summary>Tile00, northwest two-by-two small-cell patch, with empty top-left.</summary>
    internal const int Northwest = 0x00;
    /// <summary>Tile02, left end of the three-large-part cap at Y=-64.</summary>
    internal const int TopCap = 0x02;
    /// <summary>Tile0A, left end of the northern two-row strip at Y=-48.</summary>
    internal const int North = 0x0a;
    /// <summary>Tile25, left end of the upper shoulder at Y=-32. Native part34
    /// uses2E instead of the stride's2F, preserving the pinned disassembly's tiling error.</summary>
    internal const int UpperShoulder = 0x25;
    /// <summary>Tile41, separately packed overlapping right edge of the upper shoulder.</summary>
    internal const int UpperRightOverlap = 0x41;
    /// <summary>Tile60, left edge of the planet band at relative Y=0 in8C:9558.</summary>
    internal const int Lower = 0x60;
    /// <summary>Tile43, left edge of the planet band at relative Y=-16 in8C:9558.</summary>
    internal const int Upper = 0x43;
    /// <summary>Tile6E, left end of the southern two-row strip at Y=16.</summary>
    internal const int South = 0x6e;
    /// <summary>Tile86, southeast three-cell patch, with empty bottom-right.</summary>
    internal const int Southeast = 0x86;
    /// <summary>Tile88, left end of the seven-small-part bottom rim at Y=32.</summary>
    internal const int BottomRim = 0x88;

    /// <summary>Advance a horizontal strip through atlas columns; crossing sixteen
    /// columns skips the atlas row occupied by the bottom halves of large parts.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}
