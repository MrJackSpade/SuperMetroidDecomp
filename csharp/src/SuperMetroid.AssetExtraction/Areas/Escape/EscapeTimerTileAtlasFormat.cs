namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Indexed-PNG and OBJ transfer geometry for editable escape-timer characters.</summary>
public static class EscapeTimerTileAtlasFormat
{
    /// <summary>Indexed-PNG resource filename for the shared Ceres and Zebes escape-timer digit, separator, and TIME-label characters.</summary>
    public const string FileName = "escape-timer-tiles.png";
    /// <summary>Four planar bits per OBJ pixel; each eight-by-eight character occupies 32 encoded bytes.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Sixteen possible pixel indices in the four-bit artwork; runtime OBJ palettes supply display colors rather than the PNG palette.</summary>
    public const int ColorCount = 16;
    /// <summary>Twenty-five eight-by-eight characters: ten upper digit halves, ten lower halves, two separators, then three TIME-label tiles.</summary>
    public const int TileCount = 25;
    /// <summary>PNG width in pixels, 200, laying all twenty-five characters side by side in transfer order.</summary>
    public const int Width = TileCount * 8;
    /// <summary>PNG height in pixels, eight; a sixteen-pixel-tall digit is represented by separate upper and lower characters in the strip.</summary>
    public const int Height = 8;

    /// <summary>First page size, 512 bytes, containing characters 0..15.</summary>
    public const int FirstByteCount = 16 * 32;
    /// <summary>The first page's first OBJ character, 480, where the two timer-number pages start before the escape text.</summary>
    public const int FirstCharacter = 480;

    /// <summary>Second page size, 288 bytes, containing the remaining characters 16..24.</summary>
    public const int SecondByteCount = 9 * 32;
    /// <summary>The second page's first OBJ character, after the first page's sixteen.</summary>
    public const int SecondCharacter = FirstCharacter + FirstByteCount / 32;

    /// <summary>Complete planar strip size, 800 bytes, preserving the sixteen-character and nine-character page boundary.</summary>
    public const int TotalByteCount = FirstByteCount + SecondByteCount;
}
