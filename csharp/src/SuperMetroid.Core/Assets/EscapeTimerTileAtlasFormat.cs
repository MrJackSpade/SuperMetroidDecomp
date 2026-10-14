namespace SuperMetroid.Core.Assets;

/// <summary>Indexed-PNG and native OBJ transfer geometry for editable escape-timer characters.</summary>
public static class EscapeTimerTileAtlasFormat
{
    /// <summary>Indexed-PNG resource filename for the shared Ceres and Zebes escape-timer digit, separator, and TIME-label characters.</summary>
    public const string FileName = "escape-timer-tiles.png";
    /// <summary>Four planar bits per OBJ pixel; each native eight-by-eight character occupies 32 encoded bytes.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Sixteen possible pixel indices in the four-bit artwork; runtime OBJ palettes supply display colors rather than the PNG palette.</summary>
    public const int ColorCount = 16;
    /// <summary>Twenty-five eight-by-eight characters: ten upper digit halves, ten lower halves, two separators, then three TIME-label tiles.</summary>
    public const int TileCount = 25;
    /// <summary>PNG width in pixels, 200, laying all twenty-five native characters side by side in transfer order.</summary>
    public const int Width = TileCount * 8;
    /// <summary>PNG height in pixels, eight; a sixteen-pixel-tall digit is represented by separate upper and lower characters in the strip.</summary>
    public const int Height = 8;

    /// <summary>First native upload size, $0200 bytes, containing characters 0..15 from source $B0:C000.</summary>
    public const ushort FirstByteCount = 0x0200;
    /// <summary>First upload's VRAM word destination $7E00, equivalent to byte $FC00 and native timer OBJ character $1E0.</summary>
    public const ushort FirstDestinationWord = 0x7e00;

    /// <summary>Second native upload size, $0120 bytes, containing the remaining characters 16..24 from source $B0:C200.</summary>
    public const ushort SecondByteCount = 0x0120;
    /// <summary>Second upload's VRAM word destination $7F00, equivalent to byte $FE00 and native timer OBJ character $1F0.</summary>
    public const ushort SecondDestinationWord = 0x7f00;

    /// <summary>Complete planar strip size, $0320 bytes, preserving the native sixteen-character and nine-character transfer boundary.</summary>
    public const int TotalByteCount = FirstByteCount + SecondByteCount;
}
