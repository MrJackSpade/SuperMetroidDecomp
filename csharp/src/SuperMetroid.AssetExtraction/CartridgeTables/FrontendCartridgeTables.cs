namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the front-end tables the title and map extractors read.</summary>
internal static class FrontendCartridgeTables
{
    /// <summary><c>$82:C80B</c>, bank-$82 save-point map-coordinate-list pointers per area.</summary>
    internal const int SavePointMapPointers = 0x82c80b;

    /// <summary>$8C:BC5D, TitleSequenceHDMATables: sixteen pointers indexed by zoom bits 4..7.</summary>
    internal const int FixedColorPointers = 0x8cbc5d;

    /// <summary>Bank $8C containing title-sequence spritemaps referenced by bank-relative list operands.</summary>
    internal const byte TitleSpritemapBank = 0x8c;

    /// <summary>Zero spritemap-pointer sentinel that suppresses the active title-card or logo composition.</summary>
    internal const ushort TitleBlankSpritemap = 0x0000;

    /// <summary>$8C:8103, TitleSequenceSpritemaps_NintendoCopyright: bank-relative pointer to the 1994 Nintendo copyright composition.</summary>
    internal const ushort TitleCopyrightSpritemap = 0x8103;

    /// <summary>$8B:A0C7 spritemap operand in the title-logo instruction list; stores the bank-$8C logo pointer rather than the spritemap itself.</summary>
    internal const int TitleLogoSpritemapOperand = 0x8ba0c7;

    /// <summary>$8C:D781, first of four 16-byte portrait-eye draw records, in <see cref="SuperMetroid.Core.Frontend.Intro.IntroEyeFrame"/> order.</summary>
    internal const ushort IntroEyeFrameRecords = 0xd781;

    /// <summary>Each native record holds a draw function, dimensions and six words.</summary>
    internal const int IntroEyeFrameRecordStride = 0x10;
}
