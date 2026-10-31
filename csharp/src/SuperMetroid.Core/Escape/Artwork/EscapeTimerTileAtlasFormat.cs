namespace SuperMetroid.Core.Escape.Artwork;

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
    public const int FirstByteCount = 16 * 32;
    /// <summary>The first page's first OBJ character, $1E0.</summary>
    public const int FirstCharacter = EscapeTextArtworkDefinitions.TimerFirstCharacter;

    /// <summary>Second native upload size, $0120 bytes, containing the remaining characters 16..24 from source $B0:C200.</summary>
    public const int SecondByteCount = 9 * 32;
    /// <summary>The second page's first OBJ character, $1F0, after the first page's sixteen.</summary>
    public const int SecondCharacter = FirstCharacter + FirstByteCount / 32;

    /// <summary>Complete planar strip size, $0320 bytes, preserving the native sixteen-character and nine-character transfer boundary.</summary>
    public const int TotalByteCount = FirstByteCount + SecondByteCount;
}
