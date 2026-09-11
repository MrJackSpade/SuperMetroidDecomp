namespace SuperMetroid.Core.Assets;

/// <summary>Import-time identities for static map palette resources.</summary>
public static class MapStaticPalettesRomData
{
    /// <summary>$B6:F000, complete pause-menu base palette loaded by the native pause initializer.</summary>
    public const int PausePalette = 0xb6f000;
    /// <summary>$81:A3E3 copies five colors per world-map foreground record.</summary>
    public const int CopyColorCount = 5;
    /// <summary>World-map color-copy record consists of source and destination words.</summary>
    public const int CopyRecordBytes = 4;
    /// <summary>A bank cannot hold more complete four-byte records than this.</summary>
    public const int MaximumCopyRecords = 16384;
    /// <summary>$81:A546 starts executable foreground-load code after all world-map palette copy records.</summary>
    public const int WorldPaletteDataEnd = 0x81a546;
}
