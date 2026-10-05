namespace SuperMetroid.Core.Assets;

/// <summary>Stable names, extraction sources and stock anchors for the escape timer presentation.</summary>
public static class EscapeTimerPresentationDefinitions
{
    public const int Version = 1;
    public const string FileName = "escape-timer.json";
    public const int MaximumParts = 128;

    /// <summary>Bank-$80 table at <c>$80:9FD4</c> containing one spritemap pointer per decimal digit.</summary>
    public const int DigitPointerTable = 0x809fd4;

    /// <summary>$80:9FE8: first decimal digit's two-object, twelve-byte spritemap.</summary>
    private const ushort FirstDigitSpritemap = 0x9fe8;
    /// <summary>Resolves the $80:9FD4 decimal digit selection from consecutive two-object records.</summary>
    public static ushort DigitSpritemapPointer(int digit) => (uint)digit < 10
        ? (ushort)(FirstDigitSpritemap + digit * 12)
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
