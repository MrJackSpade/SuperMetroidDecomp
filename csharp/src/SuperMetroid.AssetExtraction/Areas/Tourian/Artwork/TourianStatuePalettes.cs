using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>
/// Editable entrance, eye-glow, and grey-transition RGB5 colors for the Tourian statue, which the import pre-colours the
/// statue scene's shades from (#1512).
/// </summary>
public sealed class TourianStatuePalettes
{
    /// <summary>Sixteen base-decoration inks.</summary>
    private readonly PaletteBand baseColors;
    /// <summary>Sixteen statue-body inks.</summary>
    private readonly PaletteBand statueColors;
    /// <summary>Each boss statue's eye glow, overlaid on base-decoration colors nine through twelve.</summary>
    private readonly Dictionary<TourianStatue, PaletteBand> eyeColors;
    /// <summary>Eight grey inks a released statue takes.</summary>
    private readonly PaletteBand greyColors;

    /// <summary>Keeps the base, statue, eye, and grey colors as independent paint bands.</summary>
    /// <param name="baseColors">The base-decoration band.</param>
    /// <param name="statueColors">The statue-body band.</param>
    /// <param name="eyeColors">Each boss statue's eye glow.</param>
    /// <param name="greyColors">The grey-transition band.</param>
    internal TourianStatuePalettes(PaletteBand baseColors, PaletteBand statueColors, Dictionary<TourianStatue, PaletteBand> eyeColors, PaletteBand greyColors)
    {
        this.baseColors = baseColors;
        this.statueColors = statueColors;
        this.eyeColors = eyeColors;
        this.greyColors = greyColors;
    }


    /// <summary>Gets eye colour 0–3 of <paramref name="statue"/>'s eye glow.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The statue is undefined or the colour lies past the glow's four.</exception>
    public RgbColor ResolveEye(TourianStatue statue, int color)
    {
        if (!eyeColors.TryGetValue(statue, out PaletteBand? glow))
            throw new ArgumentOutOfRangeException(nameof(statue), statue, "Undefined Tourian statue.");
        return glow.Read(color);
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
