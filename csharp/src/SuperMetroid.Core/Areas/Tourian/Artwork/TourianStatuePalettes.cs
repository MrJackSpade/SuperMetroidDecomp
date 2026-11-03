using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

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

    /// <summary>One independent installed domain: stock colors calculate; arbitrary supplied words own sparse overrides.</summary>
    internal sealed class PaletteBand
    {
        /// <summary>Calculated stock paint family used when a color has no supplied override.</summary>
        private readonly TourianStatuePaintBand band;
        /// <summary>Number of indexed colors in this band's palette geometry.</summary>
        private readonly int count;
        /// <summary>Only supplied colors that differ from the calculated stock band.</summary>
        internal readonly Dictionary<ColourCell, RgbColor> edits;
        /// <summary>Keeps a band's geometry and the supplied colors that differ from its stock paint.</summary>
        /// <param name="band">The calculated stock paint family.</param>
        /// <param name="count">The band's colour count.</param>
        /// <param name="edits">The supplied colours the import found differing from the stock paint.</param>
        internal PaletteBand(TourianStatuePaintBand band, int count, Dictionary<ColourCell, RgbColor> edits)
        {
            this.band = band;
            this.count = count;
            this.edits = edits;
        }
        /// <summary>Resolves an edited color or calculates its stock paint after validating the index.</summary>
        internal RgbColor Read(int color)
        {
            if ((uint)color >= count) throw new ArgumentOutOfRangeException(nameof(color));
            return edits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : TourianStatuePaint.Color(band, color);
        }
    }
}
