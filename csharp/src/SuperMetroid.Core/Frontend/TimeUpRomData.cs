namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge constants for an expired escape countdown's white-out.</summary>
internal static class TimeUpRomData
{
    /// <summary><c>$90:E0F5</c>: every target palette color becomes BGR555 white.</summary>
    public const ushort WhiteColor = 0x7fff;

    /// <summary><c>$82:8417</c>: state $23's gradual color-change denominator.</summary>
    public const int WhiteOutDenominator = 8;
}
