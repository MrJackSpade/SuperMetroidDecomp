namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Cartridge values and helpers of <see cref="EscapeTimerTileRomData"/> that only verification reads.</summary>
internal static class EscapeTimerTileRomDataConstants
{
    /// <summary>Final nine timer OBJ characters at <c>$B0:C200</c>.</summary>
    public const int SecondSourceAddress = 0xb0c200;
}
