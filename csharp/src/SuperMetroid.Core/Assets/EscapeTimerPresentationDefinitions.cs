namespace SuperMetroid.Core.Assets;

/// <summary>Stable names, extraction sources and stock anchors for the escape timer presentation.</summary>
public static class EscapeTimerPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "escape-timer.json";
    public const int MaximumParts = 128;

    /// <summary>Bank-$80 table at <c>$80:9FD4</c> containing one spritemap pointer per decimal digit.</summary>
    public const int DigitPointerTable = 0x809fd4;

    /// <summary>
    /// The ten immutable words at $80:9FD4-$9FE7. These select stock spritemap
    /// records; the selected glyph shapes remain editable presentation assets.
    /// </summary>
    private static readonly ushort[] DigitSpritemaps =
        [0x9fe8, 0x9ff4, 0xa000, 0xa00c, 0xa018, 0xa024, 0xa030, 0xa03c, 0xa048, 0xa054];

    /// <summary>Resolves the compiled bank-$80 spritemap pointer for a decimal digit.</summary>
    public static ushort DigitSpritemapPointer(int digit) => (uint)digit < DigitSpritemaps.Length
        ? DigitSpritemaps[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>The five-part <c>TIME</c> label spritemap at <c>$80:A060</c>.</summary>
    public const int LabelSpritemap = 0x80a060;

    /// <summary>The timer renderer inherits its spritemap pointers from bank $80.</summary>
    public const int SpritemapBank = 0x800000;

    public const string LabelFrame = "Label";
    public static string DigitFrame(int digit) => (uint)digit < 10
        ? $"Digit.{digit}"
        : throw new ArgumentOutOfRangeException(nameof(digit));

    public static readonly string[] AnchorNames = ["Label", "Minutes", "Seconds", "Centiseconds"];
}
