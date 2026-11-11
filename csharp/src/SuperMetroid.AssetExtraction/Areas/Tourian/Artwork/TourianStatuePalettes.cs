using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>
/// Editable entrance, eye-glow, and grey-transition RGB5 colors for the Tourian statue, which the import pre-colours the
/// statue scene's shades from (#1512).
/// </summary>
public sealed class TourianStatuePalettes
{
    /// <summary>Sixteen base-decoration inks.</summary>
    internal readonly PaletteBand baseColors;
    /// <summary>Sixteen statue-body inks.</summary>
    internal readonly PaletteBand statueColors;
    /// <summary>Four boss-specific eye glows overlaid on base-decoration colors nine through twelve.</summary>
    internal readonly PaletteBand eyeColors;
    /// <summary>Eight grey inks a released statue takes.</summary>
    internal readonly PaletteBand greyColors;

    /// <summary>Keeps the base, statue, eye, and grey colors as independent paint bands.</summary>
    /// <param name="baseColors">The base-decoration band.</param>
    /// <param name="statueColors">The statue-body band.</param>
    /// <param name="eyeColors">The four eye glows, glow after glow, as one band.</param>
    /// <param name="greyColors">The grey-transition band.</param>
    internal TourianStatuePalettes(PaletteBand baseColors, PaletteBand statueColors, PaletteBand eyeColors, PaletteBand greyColors)
    {
        this.baseColors = baseColors;
        this.statueColors = statueColors;
        this.eyeColors = eyeColors;
        this.greyColors = greyColors;
    }

    /// <summary>Gets eye colour 0–3 of eye glow 0–3, in Phantoon, Ridley, Draygon, Kraid order.</summary>
    public RgbColor ResolveEye(int glow, int color)
    {
        if ((uint)glow >= TourianStatueColorDefinitions.EyeGlowCount)
            throw new ArgumentOutOfRangeException(nameof(glow));
        if ((uint)color >= TourianStatueColorDefinitions.EyeColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return eyeColors.Read(glow * TourianStatueColorDefinitions.EyeColorCount + color);
    }

    /// <summary>Base-decoration colour <paramref name="color"/>, 0–15.</summary>
    public RgbColor BaseColor(int color) => baseColors.Read(color);

    /// <summary>Statue-body colour <paramref name="color"/>, 0–15.</summary>
    public RgbColor StatueColor(int color) => statueColors.Read(color);

    /// <summary>Grey colour <paramref name="color"/>, 0–7, which a released statue takes.</summary>
    public RgbColor GreyColor(int color) => greyColors.Read(color);

    /// <summary>One installed band of colours, held whole as the player may have edited them.</summary>
    internal sealed class PaletteBand
    {
        /// <summary>Every installed colour of the band, in palette order.</summary>
        private readonly RgbColor[] colors;
        /// <summary>Keeps the band's complete installed colours.</summary>
        /// <param name="colors">The band's installed colours, in palette order.</param>
        internal PaletteBand(RgbColor[] colors) => this.colors = [.. colors];
        /// <summary>Installed colour <paramref name="color"/> of the band.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The band has no such colour.</exception>
        internal RgbColor Read(int color) => (uint)color < (uint)colors.Length
            ? colors[color]
            : throw new ArgumentOutOfRangeException(nameof(color));
    }
}
