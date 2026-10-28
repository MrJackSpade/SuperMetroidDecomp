using SuperMetroid.Core.Areas.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Editable entrance, eye-glow, and grey-transition RGB5 colors for the Tourian statue.</summary>
public sealed class TourianStatuePalettes
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("TourianStatueColorCatalog-v1", content =>
        {
            Span<RgbColor> row = stackalloc RgbColor[TourianStatueColorDefinitions.BaseColorCount];
            for (int color = 0; color < row.Length; color++) row[color] = ResolveBase(color);
            content.AppendColors("baseColors", row);
            content.AppendColors("statueColors", statueColors.Words());
            for (int color = 0; color < TourianStatueColorDefinitions.GreyColorCount; color++) row[color] = ResolveGrey(color);
            content.AppendColors("greyColors", row[..TourianStatueColorDefinitions.GreyColorCount]);
            content.AppendColorFrames("eyeColors", Enumerable.Range(0, TourianStatueColorDefinitions.EyeRowCount).Select(row =>
                Enumerable.Range(0, TourianStatueColorDefinitions.EyeColorCount).Select(color => ResolveEye(row, color)).ToArray()).ToArray());
        });

    /// <summary>Sixteen base-decoration inks installed at OBJ palette seven.</summary>
    internal readonly PaletteBand baseColors;
    /// <summary>Sixteen statue-body inks installed at OBJ palette two.</summary>
    internal readonly PaletteBand statueColors;
    /// <summary>Four boss-specific eye rows overlaid on base-decoration colors nine through twelve.</summary>
    internal readonly PaletteBand eyeColors;
    /// <summary>Eight grey inks installed at the animation instruction's CGRAM destination.</summary>
    internal readonly PaletteBand greyColors;

    /// <summary>Separates the supplied base, statue, eye, and grey colors into independent paint bands.</summary>
    internal TourianStatuePalettes(RgbColor[] baseColors, RgbColor[] statueColors,
        RgbColor[][] eyeColors, RgbColor[] greyColors)
    {
        this.baseColors = new(TourianStatuePaintBand.Base, baseColors);
        this.statueColors = new(TourianStatuePaintBand.Statue, statueColors);
        this.eyeColors = new(TourianStatuePaintBand.Eye, eyeColors.SelectMany(row => row).ToArray());
        this.greyColors = new(TourianStatuePaintBand.Grey, greyColors);
    }

    /// <summary>Gets packed RGB5 base-decoration ink 0–15 corresponding to $AA:D785, including its retained transparent-slot word.</summary>
    public RgbColor ResolveBase(int color) => baseColors.Read(color);
    /// <summary>Gets packed RGB5 eye ink 0–3 from row 0–3 of $86:B91E, in Phantoon, Ridley, Draygon, Kraid order.</summary>
    public RgbColor ResolveEye(int row, int color)
    {
        if ((uint)row >= TourianStatueColorDefinitions.EyeRowCount)
            throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= TourianStatueColorDefinitions.EyeColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return eyeColors.Read(row * TourianStatueColorDefinitions.EyeColorCount + color);
    }
    /// <summary>Gets packed RGB5 ink 0–7 from the statue's grey-transition band corresponding to $87:839C; destination and transition ordering remain animation-owned.</summary>
    public RgbColor ResolveGrey(int color) => greyColors.Read(color);

    /// <summary>Installs all sixteen base-decoration inks at CGRAM 240–255 and all sixteen statue inks at 160–175, matching the entrance initializer's two OBJ palette bands.</summary>
    public void ApplyEntrance(GameplayColours colours)
    {
        baseColors.Apply(new AreaColours(colours).TourianStatueBase);
        statueColors.Apply(new AreaColours(colours).TourianStatue);
    }

    /// <summary>Installs the selected four-color eye glow at CGRAM 249–252 without altering boss completion or animated-tile state.</summary>
    /// <param name="colours">The colours receiving the selected eye glow.</param>
    /// <param name="doubledBossParameter">Native doubled selector: Phantoon 0, Ridley 2, Draygon 4, or Kraid 6.</param>
    public void ApplyEye(GameplayColours colours, int doubledBossParameter)
    {
        if (doubledBossParameter > 6 || (doubledBossParameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(doubledBossParameter));
        Ensure.NotNull(colours);
        for (int color = 0; color < TourianStatueColorDefinitions.EyeColorCount; color++)
            new AreaColours(colours).TourianStatueEye.Write(color, ResolveEye(doubledBossParameter >> 1, color));
    }

    /// <summary>Installs the eight selected grey colours in <paramref name="destination"/>, the statue's; this method does not own target-palette fade progression.</summary>
    /// <param name="destination">The statue's colours the grey replaces.</param>
    public void ApplyGrey(ColourRun destination) => greyColors.Apply(destination);

    /// <summary>One independent installed domain: stock colors calculate; arbitrary supplied words own sparse overrides.</summary>
    internal sealed class PaletteBand
    {
        /// <summary>Calculated stock paint family used when a color has no supplied override.</summary>
        private readonly TourianStatuePaintBand band;
        /// <summary>Number of indexed colors in this band's native palette geometry.</summary>
        private readonly int count;
        /// <summary>Only supplied colors that differ from the calculated stock band.</summary>
        internal readonly Dictionary<ColourCell, RgbColor> edits = [];
        /// <summary>Compares supplied colors with stock paint and retains independent edits.</summary>
        internal PaletteBand(TourianStatuePaintBand band, ReadOnlySpan<RgbColor> supplied)
        {
            this.band = band;
            count = supplied.Length;
            for (int color = 0; color < count; color++)
                if (supplied[color] != TourianStatuePaint.Color(band, color)) edits.Add(new ColourCell(0, color), supplied[color]);
        }
        /// <summary>Resolves an edited color or calculates its stock paint after validating the index.</summary>
        internal RgbColor Read(int color)
        {
            if ((uint)color >= count) throw new ArgumentOutOfRangeException(nameof(color));
            return edits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : TourianStatuePaint.Color(band, color);
        }
        /// <summary>Materializes the selected band for content hashing in indexed color order.</summary>
        internal RgbColor[] Words()
        {
            var result = new RgbColor[count];
            for (int color = 0; color < count; color++) result[color] = Read(color);
            return result;
        }
        /// <summary>Writes every selected color into the supplied CGRAM palette slice.</summary>
        internal void Apply(ColourRun destination)
        {
            for (int color = 0; color < count; color++) destination.Write(color, Read(color));
        }
    }
}
