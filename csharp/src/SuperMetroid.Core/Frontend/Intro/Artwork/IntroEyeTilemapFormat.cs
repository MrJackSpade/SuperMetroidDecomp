namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Stable identities and calculated atlas layout for the four portrait eye drawings. The three selected patch anchors and display design identify authored portrait content; changing them invents different drawings. Timing and pixels are outside this retained scope.</summary>
public static class IntroEyeTilemapFormat
{
    /// <summary>Supported eye-frame document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of ordered eye drawings in the blink selector.</summary>
    public const int FrameCount = 4;
    /// <summary>Number of BG cells across each eye rectangle.</summary>
    public const int Columns = 3;
    /// <summary>Number of BG cells down each eye rectangle.</summary>
    public const int Rows = 2;
    /// <summary>Number of row-major tilemap cells in each frame.</summary>
    public const int CellsPerFrame = Columns * Rows;
    /// <summary>$8C:D785/D795/D7A5/D7B5: second tile row begins16 characters after the first; each selected patch uses the native 16-column artwork atlas.</summary>
    internal const int NativeRowStride = 16;
    /// <summary>$8C:D785: open-eye patch starts at tile389 of the installed95:F90E atlas.</summary>
    private const int OpenTile = 0x389;
    /// <summary>$8C:D795: half-open-eye patch starts at tile31D of the same atlas.</summary>
    private const int HalfOpenTile = 0x31d;
    /// <summary>$8C:D7A5: closed-eye patch starts at tile33A; deadpan patch atD7B5 lies immediately to its right.</summary>
    private const int ClosedTile = 0x33a;
    /// <summary>$8C:D785-D7BF: selected portrait palette three, no priority or flips.</summary>
    private const int PortraitPalette = 3;
    /// <summary>Selects the native open, half-open, closed, or adjacent deadpan patch anchor.</summary>
    internal static ushort FirstWord(int frame)
    {
        int tile = frame switch
        {
            0 => OpenTile,
            1 => HalfOpenTile,
            2 => ClosedTile,
            3 => ClosedTile + Columns,
            _ => throw new ArgumentOutOfRangeException(nameof(frame)),
        };
        return SnesBgTilemapWord.Create(tile, PortraitPalette, false, default).Raw;
    }
    /// <summary>JSON filename containing the four editable eye rectangles.</summary>
    public const string FileName = "intro-samus-eye-frames.json";

    /// <summary>Builds the stable frame identity for a zero-based selector index.</summary>
    public static string FrameId(int index)
    {
        if ((uint)index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return $"eye-frame-{index}";
    }
}
