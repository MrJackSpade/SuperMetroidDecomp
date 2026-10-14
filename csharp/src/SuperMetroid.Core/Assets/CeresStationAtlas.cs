namespace SuperMetroid.Core.Assets;

/// <summary>Native tile anchors for the station's strips and irregular edge patches.</summary>
internal static class CeresStationAtlas
{
    /// <summary>TileE6, top strip at Y=-48 in8C:9150.</summary>
    internal const int Top = 0xe6;
    /// <summary>TileEC, left end of the shoulder strip at Y=-32.</summary>
    internal const int Shoulder = 0xec;
    /// <summary>Tile107, left end of the upper-middle strip at Y=-16.</summary>
    internal const int UpperMiddle = 0x107;
    /// <summary>Tile124, left end of the middle strip at Y=0.</summary>
    internal const int Middle = 0x124;
    /// <summary>Tile140, left end of the lower strip at Y=16.</summary>
    internal const int Lower = 0x140;
    /// <summary>TileF0, small extension beyond the right end of the middle strip.</summary>
    internal const int RightTip = 0xf0;
    /// <summary>TileF1, two adjacent lower-right edge cells followed by the
    /// separately packed downward continuation of the neighboring pair.</summary>
    internal const int LowerRightEdge = 0xf1;
    /// <summary>Tile15B, two adjacent small cells on the right of the lower center patch.</summary>
    internal const int LowerRightPair = 0x15b;
    /// <summary>Tile14B, three-cell left edge, its lower-right continuation in the
    /// next atlas row, and an adjoining large center patch at column+3.</summary>
    internal const int LowerLeftEdge = 0x14b;
    /// <summary>At a sixteen-column boundary, skip the row of lower tile halves.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}
