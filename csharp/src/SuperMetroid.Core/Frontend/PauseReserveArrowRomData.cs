namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$82 energy-arrow tile and color definitions.</summary>
internal static class PauseReserveArrowRomData
{
    /// <summary>$82:AD5D, animated BG palette-six color-six sequence.</summary>
    public const int Color6Table = 0x82ad5d;
    /// <summary>$82:AD9D, animated BG palette-six color-eleven sequence.</summary>
    public const int Color11Table = 0x82ad9d;
    /// <summary>$82:AD34 masks the native eight-bit NMI counter to a 32-frame cycle.</summary>
    public const int FrameMask = 31;
    /// <summary>$82:AD3C writes palette six, color six.</summary>
    public const int Color6Index = 102;
    /// <summary>$82:AD43 writes palette six, color eleven.</summary>
    public const int Color11Index = 107;
    /// <summary>$82:ADE4/$ADF6 solid-arrow color six.</summary>
    public const ushort SolidColor6 = 0x0156;
    /// <summary>$82:ADDD/$ADEF solid-arrow color eleven.</summary>
    public const ushort SolidColor11 = 0x039e;
    /// <summary>$82:AE14 enables the arrow by selecting BG palette six.</summary>
    public const int EnabledPalette = 6;
    /// <summary>$82:AE59 disables glow by selecting BG palette seven.</summary>
    public const int DisabledPalette = 7;
    /// <summary>$82:AE0D vertical arrow starts at equipment tilemap byte $102.</summary>
    public const int VerticalStart = 0x102;
    /// <summary>$82:AE1D vertical arrow advances one 32-word row.</summary>
    public const int RowStride = 0x40;
    /// <summary>$82:AE07/$AE21-$AE23 writes eight vertical words.</summary>
    public const int VerticalCount = 8;
    /// <summary>$82:AE2F horizontal arrow starts at equipment tilemap byte $302.</summary>
    public const int HorizontalStart = 0x302;
    /// <summary>$82:AE29/$AE3F-$AE41 writes two horizontal words.</summary>
    public const int HorizontalCount = 2;
}
