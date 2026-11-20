namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Stable filenames and exact four-bit PNG transfer geometry for both melts.</summary>
public static class CrocomireMeltingArtworkFormat
{
    /// <summary>Import-data indexed PNG filename for the first dissolve image, assembled from the native bank-$A4 copies beginning at $A07D.</summary>
    public const string FleshFileName = "crocomire-melt-flesh.png";
    /// <summary>Import-data indexed PNG filename for the second, eroded dissolve image, assembled from the native bank-$A4 copies beginning at $AC7D.</summary>
    public const string BareSkullFileName = "crocomire-melt-bare-skull.png";
    /// <summary>Import-data JSON filename for the first image's 256 BG2 references, corresponding to $A4:9C79.</summary>
    public const string FleshTilemapFileName = "crocomire-melt-flesh-tiles.json";
    /// <summary>Import-data JSON filename for the second image's 256 BG2 references, corresponding to $A4:9E7B.</summary>
    public const string BareSkullTilemapFileName = "crocomire-melt-bare-skull-tiles.json";
    /// <summary>$0E20 compiled 4bpp bytes, or 113 characters, for the first scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int FleshByteCount = 3616;
    /// <summary>$1020 compiled 4bpp bytes, or 129 characters, for the second scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int BareSkullByteCount = 4128;
    /// <summary>
    /// Supported visual tile-layout schema revision, checked before compiling packed SNES BG words: 2 declares the layout's
    /// grid as the 32-by-8 one it is read in (#2564).
    /// </summary>
    public const int TilemapVersion = 2;
    /// <summary>A melt layout's width in cells: one row of the image.</summary>
    public const int TilemapWidth = 32;
    /// <summary>A melt layout's height in cells.</summary>
    public const int TilemapHeight = 8;
    /// <summary>The cells of a melt layout, row by row.</summary>
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
}
