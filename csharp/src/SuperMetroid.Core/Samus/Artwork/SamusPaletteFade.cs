using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Shared RGB5 whitening used by Samus charge and death palettes.</summary>
internal static class SamusPaletteFade
{
    /// <summary>Moves each channel toward white by shade/8, rounding down.</summary>
    /// <remarks>Original bank9B suited rows9820..9B1F and suitless rowsA120..A21F
    /// use floor((base*(8-shade)+31*shade)/8), including color zero. Charge
    /// selects shades0..3; death uses0..7. Each numerator is0..248, with no
    /// saturation, wrap or cross-channel carry. Base colors remain asset inputs;
    /// independently edited target channels override the calculation.</remarks>
    internal static RgbColor EighthTowardWhite(RgbColor basis, int shade)
    {
        if ((uint)shade >= 8) throw new ArgumentOutOfRangeException(nameof(shade));
        int red = ((basis.Red) * (8 - shade) + 31 * shade) / 8;
        int green = ((basis.Green) * (8 - shade) + 31 * shade) / 8;
        int blue = ((basis.Blue) * (8 - shade) + 31 * shade) / 8;
        return new RgbColor(red, green, blue);
    }
}
