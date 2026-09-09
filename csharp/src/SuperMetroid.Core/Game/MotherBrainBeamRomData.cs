namespace SuperMetroid.Core.Game;

/// <summary>Bank-$88/$AD identities for Mother Brain's rainbow window, not her sprite palettes.</summary>
public static class MotherBrainBeamRomData
{
    /// <summary>Mutually exclusive targets of $AD:DE5F, indexed by the two edge quadrants.</summary>
    public enum Direction { Down, Up, Right, Retain, Unsupported }
    /// <summary>$AD:DE5F-$DE7D dispatcher order; null pointers remain explicit unsupported entries.</summary>
    public static ReadOnlySpan<Direction> QuadrantDirections =>
    [
        Direction.Down, Direction.Right, Direction.Unsupported, Direction.Unsupported,
        Direction.Unsupported, Direction.Up, Direction.Up, Direction.Unsupported,
        Direction.Unsupported, Direction.Unsupported, Direction.Up, Direction.Retain,
        Direction.Down, Direction.Unsupported, Direction.Unsupported, Direction.Down,
    ];
    /// <summary>$88:E833, Set_RainbowBeam_ColorMathSubscreenBackdropColor.table; signed terminator.</summary>
    public const int ColorTable = 0x88e833;
    /// <summary>$88:E7FD increments the byte cursor four times, skipping every other color word.</summary>
    public const int ColorStride = 4;
    /// <summary>$88:E76C-$E776 initial fixed color: red 0, green 7, blue 15.</summary>
    public const ushort InitialColor = 0x3ce0;
    /// <summary>$AD:DE24 adds fourteen pixels to the low byte of the head X coordinate.</summary>
    public const int MouthXOffset = 14;
    /// <summary>$AD:DE35 adds five pixels to the head Y coordinate.</summary>
    public const int MouthYOffset = 5;
    /// <summary>$AD:DEFE/$DF27, first beam data scanline following the two sixteen-line HUD entries.</summary>
    public const int FirstLine = 32;
    /// <summary>$AD:DF57, exclusive bottom of the native beam's overscan work area.</summary>
    public const int EndLine = 232;
    /// <summary>$7E:9D00 data table; first two words are reserved by the indirect header.</summary>
    public const int DataPrefixWords = 2;
    /// <summary>$AD:DED4 inverted WH0/WH1 endpoints, the cartridge's empty-window word.</summary>
    public const ushort EmptyWindow = 0x00ff;
    /// <summary>$AD:DF1E sets the right endpoint to the rightmost screen pixel.</summary>
    public const ushort RightScreenEdge = 0xff00;
    /// <summary>$AD:DEAA/$E1D1 first indirect data run uses $F0: 112 per-line transfers.</summary>
    public const int FirstRunLines = 112;
    /// <summary>$AD:DEBF second rightward run starts at table byte $EC, skipping four words after the first run.</summary>
    public const int RightSecondRunWord = 0xec / 2;
    /// <summary>$AD:E1E6 second downward run starts at table byte $E6, skipping one word.</summary>
    public const int DownSecondRunWord = 0xe6 / 2;
}
