namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Stable filenames and exact four-bit PNG transfer geometry for both melts.</summary>
public static class CrocomireMeltingArtworkFormat
{
    /// <summary>Installed indexed PNG filename for the first dissolve image, assembled from the native bank-$A4 copies beginning at $A07D.</summary>
    public const string FirstFileName = "crocomire-melt-first.png";
    /// <summary>Installed indexed PNG filename for the second, eroded dissolve image, assembled from the native bank-$A4 copies beginning at $AC7D.</summary>
    public const string SecondFileName = "crocomire-melt-second.png";
    /// <summary>Installed editable JSON filename for the first image's 256 BG2 references, corresponding to $A4:9C79.</summary>
    public const string FirstTilemapFileName = "crocomire-melt-first-tiles.json";
    /// <summary>Installed editable JSON filename for the second image's 256 BG2 references, corresponding to $A4:9E7B.</summary>
    public const string SecondTilemapFileName = "crocomire-melt-second-tiles.json";
    /// <summary>$0E20 compiled 4bpp bytes, or 113 characters, for the first scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int FirstByteCount = 3616;
    /// <summary>$1020 compiled 4bpp bytes, or 129 characters, for the second scratch image; bytes beyond the native overlapping-copy extent must remain zero.</summary>
    public const int SecondByteCount = 4128;
    /// <summary>Supported visual tile-layout schema revision, checked before compiling packed SNES BG words.</summary>
    public const int TilemapVersion = 1;
    /// <summary>Required JSON width declaration of 16; cells retain native linear order rather than being rearranged into 16-column runtime BG rows.</summary>
    public const int TilemapWidth = 16;
    /// <summary>Required JSON height declaration of 16, combining with the width to bound the native 256-word illustration payload.</summary>
    public const int TilemapHeight = 16;
    /// <summary>256 ordered BG2 references, representing eight 32-cell native rows despite the editable document's 16-by-16 dimension declaration.</summary>
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
}
