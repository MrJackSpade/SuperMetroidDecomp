namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge locations of the two consecutive escape-timer character pages.</summary>
public static class EscapeTimerTileRomData
{
    /// <summary>First sixteen timer OBJ characters at <c>$B0:C000</c>.</summary>
    public const int FirstSourceAddress = 0xb0c000;

    /// <summary>Final nine timer OBJ characters at <c>$B0:C200</c>.</summary>
    public const int SecondSourceAddress = 0xb0c200;
}
