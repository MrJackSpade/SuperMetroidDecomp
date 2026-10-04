namespace SuperMetroid.Core.Frontend;

/// <summary>Native ending-logo palette source traversal; RGB5 colors remain editable artwork.</summary>
internal static class EndingLogoPalettePointerDefinitions
{
    /// <summary>$8B:E5E7, sixteen pairs of reverse-copy bank-$8C palette pointers.</summary>
    public const int NativeTableAddress = 0x8be5e7;
    /// <summary>$8B:E5E7 selects the last color of BG palette10 at$8C:F3E7.</summary>
    private const int FirstBackgroundEnd = 0xf3e7;
    /// <summary>$8B:E5E9 selects the last color of sprite palette1 at$8C:F007.</summary>
    private const int FirstSpriteEnd = 0xf007;
    /// <summary>Sixteen RGB5 words occupy32 bytes; E58A copies each palette backwards.</summary>
    private const int PaletteBytes = 16 * sizeof(ushort);

    /// <summary>For steps0..15, BG traverses palettes10..1 while sprite traverses1..10
    /// (hex palette identities). Each source is the last color, not the palette start.
    /// Independently checked against all32 native words for #1165; no generated cache.</summary>
    public static ushort Source(int step, int palette)
    {
        if ((uint)step >= EndingLogoDefinitions.PaletteSteps)
            throw new ArgumentOutOfRangeException(nameof(step));
        return palette switch
        {
            0 => (ushort)(FirstBackgroundEnd - step * PaletteBytes),
            1 => (ushort)(FirstSpriteEnd + step * PaletteBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }
}