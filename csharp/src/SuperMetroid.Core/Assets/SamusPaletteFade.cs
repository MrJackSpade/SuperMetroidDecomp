namespace SuperMetroid.Core.Assets;

/// <summary>Shared RGB5 whitening used by Samus charge and death palettes.</summary>
internal static class SamusPaletteFade
{
    /// <summary>Moves each channel toward white by shade/8, rounding down.</summary>
    /// <remarks>Original bank9B suited rows9820..9B1F and suitless rowsA120..A21F
    /// use floor((base*(8-shade)+31*shade)/8), including color zero. Charge
    /// selects shades0..3; death uses0..7. Each numerator is0..248, with no
    /// saturation, wrap or cross-channel carry. Base colors remain asset inputs;
    /// independently edited target channels override the calculation.</remarks>
    internal static ushort EighthTowardWhite(ushort basis, int shade)
    {
        if (basis > 0x7fff) throw new ArgumentOutOfRangeException(nameof(basis));
        if ((uint)shade >= 8) throw new ArgumentOutOfRangeException(nameof(shade));
        int red = ((basis & 31) * (8 - shade) + 31 * shade) / 8;
        int green = ((basis >> 5 & 31) * (8 - shade) + 31 * shade) / 8;
        int blue = ((basis >> 10) * (8 - shade) + 31 * shade) / 8;
        return (ushort)(red | green << 5 | blue << 10);
    }
}
