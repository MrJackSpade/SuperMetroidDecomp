using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Where the cartridge stores the escape timer's two character pages, which only verification's oracles read.</summary>
internal static class EscapeTimerTileSourceConstants
{
    /// <summary>The first sixteen timer characters, at <c>$B0:C000</c>, where extraction reads the whole sheet from.</summary>
    internal static int FirstSourceAddress => (int)EscapeSourceDefinitions.TimerCharacters;

    /// <summary>The final nine timer characters, at <c>$B0:C200</c>.</summary>
    public const int SecondSourceAddress = 0xb0c200;
}
