using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the escape warning text and timer spritemaps the escape extractors read.</summary>
[CpuBusBoundary("catalog: escape cartridge tables")]
internal static class EscapeCartridgeTables
{
    /// <summary>$A6:C450 TypewriterText_CeresEscapeTimer: the Ceres escape warning program.</summary>
    internal const int CeresWarningText = 0xa6c450;
    /// <summary>$A6:C49C TypewriterText_ZebesEscapeTimer: the Zebes escape warning program.</summary>
    internal const int ZebesWarningText = 0xa6c49c;
    /// <summary>Bank $80, which holds the escape timer's spritemaps.</summary>
    internal const int TimerSpritemapBank = 0x800000;
    /// <summary>$80:A060: the five-part <c>TIME</c> label spritemap.</summary>
    internal const int TimerLabelSpritemap = 0x80a060;
    /// <summary>$80:9FE8: the first decimal digit's two-object spritemap; $80:9FD4 selects consecutive records.</summary>
    internal const ushort FirstTimerDigitSpritemap = 0x9fe8;
    /// <summary>Bytes in one digit's two-object spritemap record.</summary>
    internal const int TimerDigitSpritemapStride = 12;
}
