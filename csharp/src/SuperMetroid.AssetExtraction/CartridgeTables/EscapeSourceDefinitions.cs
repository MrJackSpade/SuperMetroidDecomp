using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the escape warning text and timer spritemaps the escape extractors read.</summary>
internal static class EscapeSourceDefinitions
{
    /// <summary>$A6:C450 TypewriterText_CeresEscapeTimer: the Ceres escape warning program.</summary>
    internal static readonly SnesAddress CeresWarningText = new(0xa6, 0xc450);
    /// <summary>$A6:C49C TypewriterText_ZebesEscapeTimer: the Zebes escape warning program.</summary>
    internal static readonly SnesAddress ZebesWarningText = new(0xa6, 0xc49c);
    /// <summary>Bank $80, which holds the escape timer's spritemaps.</summary>
    internal static readonly SnesAddress TimerSpritemapBank = new(0x80, 0x0000);
    /// <summary>$80:A060: the five-part <c>TIME</c> label spritemap.</summary>
    internal static readonly SnesAddress TimerLabelSpritemap = new(0x80, 0xa060);
    /// <summary>$80:9FE8: the first decimal digit's two-object spritemap; $80:9FD4 selects consecutive records.</summary>
    internal const ushort FirstTimerDigitSpritemap = 0x9fe8;
    /// <summary>Bytes in one digit's two-object spritemap record.</summary>
    internal const int TimerDigitSpritemapStride = 12;
    /// <summary>$B7:DA00: the escape warning text's characters, which the Ceres and Zebes warnings both copy.</summary>
    internal static readonly SnesAddress TextCharacters = new(0xb7, 0xda00);
    /// <summary>$B0:BA00: the Ceres escape doors' characters.</summary>
    internal static readonly SnesAddress CeresEscapeDoorCharacters = new(0xb0, 0xba00);
    /// <summary>$B0:C000: the escape timer's characters, its two pages one after the other.</summary>
    internal static readonly SnesAddress TimerCharacters = new(0xb0, 0xc000);
    /// <summary>Where each Ceres escape character page starts, in <c>CeresEscapeTileArtworkDefinitions.All</c> order.</summary>
    internal static readonly SnesAddress[] CeresEscapePages = [TextCharacters, CeresEscapeDoorCharacters];
}
