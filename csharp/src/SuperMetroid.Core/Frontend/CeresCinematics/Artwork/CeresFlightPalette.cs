using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The Ceres approach's full, editable screen colours (#1464): its background sets and sprite sets, the colours the
/// painting repeats or interpolates calculated from their independent inputs, and every supplied colour that differs
/// from that calculation kept as an edit.
/// </summary>
public sealed class CeresFlightPalette
{
    /// <summary>The background set whose colours one to eight interpolate between their two endpoints.</summary>
    private const int RampSet = 6;

    /// <summary>The ramp's first endpoint colour.</summary>
    private const int RampFirstColour = 1;

    /// <summary>The ramp's last endpoint colour.</summary>
    private const int RampLastColour = 8;

    /// <summary>The background sets, from the first, whose visible colours repeat the neutral fill.</summary>
    private const int FillSets = 4;

    /// <summary>The first visible colour of the first background set that the neutral fill covers; the colours before it are independent.</summary>
    private const int FirstFillColour = 4;

    /// <summary>The sprite set whose colours are the sprites' own, not repeats of the background set of the same identity.</summary>
    private const int OwnSpriteSet = 5;

    /// <summary>The sprite set holding the five hue colours that are the sprites' own.</summary>
    private const int HueSpriteSet = 7;

    /// <summary>The first of the sprites' own hue colours.</summary>
    private const int FirstHueColour = 9;

    /// <summary>The last of the sprites' own hue colours.</summary>
    private const int LastHueColour = 13;

    /// <summary>The backdrop, the first background set's transparent colour.</summary>
    private static readonly ScreenColourCell Backdrop = new(ScreenColourOwner.Backgrounds, 0, 0);

    /// <summary>The transparent colour every other background set repeats.</summary>
    private static readonly ScreenColourCell BackgroundTransparent = new(ScreenColourOwner.Backgrounds, 1, 0);

    /// <summary>The transparent colour every sprite set repeats.</summary>
    private static readonly ScreenColourCell SpriteTransparent = new(ScreenColourOwner.Sprites, 0, 0);

    /// <summary>The neutral fill the first background sets repeat.</summary>
    private static readonly ScreenColourCell NeutralFill = new(ScreenColourOwner.Backgrounds, 0, 2);

    /// <summary>The ramp's first endpoint.</summary>
    private static readonly ScreenColourCell RampStart = new(ScreenColourOwner.Backgrounds, RampSet, RampFirstColour);

    /// <summary>The ramp's last endpoint.</summary>
    private static readonly ScreenColourCell RampEnd = new(ScreenColourOwner.Backgrounds, RampSet, RampLastColour);

    /// <summary>Independent painted colors; only repeated sections and endpoint-defined ramps are calculated.</summary>
    internal readonly Dictionary<ScreenColourCell, RgbColor> colors = [];
    /// <summary>Supplied colours that differ from those calculated relationships remain independent.</summary>
    internal readonly Dictionary<ScreenColourCell, RgbColor> edits = [];

    /// <summary>Separates the supplied colours into independent colors and edited exceptions.</summary>
    /// <param name="supplied">The supplied colours by owner, which the import pipeline decoded.</param>
    internal CeresFlightPalette(ScreenColours supplied)
    {
        Ensure.NotNull(supplied);
        foreach (ScreenColourCell cell in Cells())
        {
            ScreenColourCell source = Source(cell);
            if (!IsRampInterior(source)) colors.TryAdd(source, supplied[source]);
        }
        foreach (ScreenColourCell cell in Cells())
            if (supplied[cell] != Calculate(cell)) edits.Add(cell, supplied[cell]);
    }

    /// <summary>The selected colours by owner, including independent resource edits.</summary>
    public ScreenColours Colours => ScreenColours.From(cell => edits.TryGetValue(cell, out RgbColor edited) ? edited : Calculate(cell));

    /// <summary>Every cell of the screen, the background sets' before the sprite sets'.</summary>
    private static IEnumerable<ScreenColourCell> Cells()
    {
        foreach (ScreenColourOwner owner in Enum.GetValues<ScreenColourOwner>())
        for (int set = 0; set < ColourSets.SetCount; set++)
        for (int colour = 0; colour < ColourSets.SetColours; colour++)
            yield return new ScreenColourCell(owner, set, colour);
    }

    /// <summary>
    /// $8C:E5E9: the sprite sets repeat the background sets of the same identity in every visible colour except the
    /// sprites' own set and their own five hue colours; every transparent colour repeats its owner's, and the first
    /// background sets repeat the neutral fill. These are colour relationships, not exemptions for their painted inputs.
    /// </summary>
    private static ScreenColourCell Source(ScreenColourCell cell)
    {
        if (cell.Colour == 0)
            return cell == Backdrop ? Backdrop : cell.Owner == ScreenColourOwner.Backgrounds ? BackgroundTransparent : SpriteTransparent;
        if (cell.Owner == ScreenColourOwner.Sprites && cell.Set != OwnSpriteSet &&
            !(cell.Set == HueSpriteSet && cell.Colour is >= FirstHueColour and <= LastHueColour))
            cell = cell with { Owner = ScreenColourOwner.Backgrounds };
        if (cell.Owner == ScreenColourOwner.Backgrounds && cell.Set < FillSets && (cell.Set > 0 || cell.Colour >= FirstFillColour))
            return NeutralFill;
        return cell;
    }

    /// <summary>$8C:E6AB-E6BA and E7AB-E7BA: eight colors interpolate RGB5 endpoints with nearest-integer rounding.</summary>
    private static bool IsRampInterior(ScreenColourCell cell) =>
        cell.Owner == ScreenColourOwner.Backgrounds && cell.Set == RampSet && cell.Colour is > RampFirstColour and < RampLastColour;

    /// <summary>Calculates repeated colours or interpolated ramp colors from independent inputs.</summary>
    private RgbColor Calculate(ScreenColourCell cell)
    {
        ScreenColourCell source = Source(cell);
        if (!IsRampInterior(source)) return colors[source];
        int step = source.Colour - RampFirstColour;
        int steps = RampLastColour - RampFirstColour;
        RgbColor first = colors[RampStart], last = colors[RampEnd];
        return first.Zip(last, (_, start, end) => (start * (steps - step) + end * step + steps / 2) / steps);
    }

    /// <summary>Replaces every colour of the screen with the selected approach palette; cinematic timing, brightness, and fades remain the caller's responsibility.</summary>
    /// <param name="colours">The screen's colours.</param>
    public void LoadTo(ScreenPalette colours) => Colours.LoadTo(colours);
}
