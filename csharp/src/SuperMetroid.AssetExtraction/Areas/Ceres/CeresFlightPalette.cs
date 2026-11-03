using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>
/// The Ceres approach's full, editable colours as the import reads them (#1464, #1512): its background colours then its
/// sprite colours in the cartridge's colour order, the colours the painting repeats or interpolates calculated from their
/// independent inputs, and every supplied colour that differs from that calculation kept as an edit. The import
/// pre-colours the Ceres scenes' pictures from these colours; the game keeps only their backdrop.
/// </summary>
public sealed class CeresFlightPalette
{
    /// <summary>Every colour of the approach: its background colours, then its sprite colours.</summary>
    internal const int ColourCount = 2 * OwnerColours;

    /// <summary>The colours of each owner, the backgrounds' or the sprites'.</summary>
    internal const int OwnerColours = 128;

    /// <summary>The colours of one colour set.</summary>
    internal const int SetColours = 16;

    /// <summary>The backdrop, the first background set's transparent colour.</summary>
    private const int BackdropColour = 0;

    /// <summary>The transparent colour every other background set repeats.</summary>
    private const int BackgroundTransparent = SetColours;

    /// <summary>The transparent colour every sprite set repeats.</summary>
    private const int SpriteTransparent = OwnerColours;

    /// <summary>The neutral fill the first background sets repeat.</summary>
    private const int NeutralFill = 2;

    /// <summary>The ramp's first endpoint, the sixth background set's first visible colour.</summary>
    private const int RampStart = 6 * SetColours + 1;

    /// <summary>The ramp's last endpoint.</summary>
    private const int RampEnd = 6 * SetColours + 8;

    /// <summary>
    /// The first background sets' visible colours after the first set's independent ones, which repeat the neutral fill:
    /// the first, and how many.
    /// </summary>
    private static readonly (int First, int Count) NeutralFillColours = (4, 60);

    /// <summary>
    /// The sprite colours that are the sprites' own: the fifth set's and five hue colours of the seventh, each its first
    /// colour and count. Every other visible sprite colour repeats the background colour of the same set and colour.
    /// </summary>
    private static readonly (int First, int Count)[] SpritesOwnColours =
    [
        (OwnerColours + 5 * SetColours, 16),
        (OwnerColours + 7 * SetColours + 9, 5),
    ];

    /// <summary>Independent painted colours by colour; only repeated sections and endpoint-defined ramps are calculated.</summary>
    internal readonly Dictionary<int, RgbColor> colors = [];
    /// <summary>Supplied colours that differ from those calculated relationships remain independent, by colour.</summary>
    internal readonly Dictionary<int, RgbColor> edits = [];

    /// <summary>Separates the supplied colours into independent colours and edited exceptions.</summary>
    /// <param name="supplied">The approach's 256 colours in the cartridge's colour order.</param>
    /// <exception cref="InvalidDataException">The approach does not supply exactly 256 colours.</exception>
    internal CeresFlightPalette(ReadOnlySpan<RgbColor> supplied)
    {
        if (supplied.Length != ColourCount)
            throw new InvalidDataException($"The Ceres flight palette requires {ColourCount} colours.");
        for (int colour = 0; colour < ColourCount; colour++)
        {
            int source = Source(colour);
            if (!IsRampInterior(source)) colors.TryAdd(source, supplied[source]);
        }
        for (int colour = 0; colour < ColourCount; colour++)
            if (supplied[colour] != Calculate(colour)) edits.Add(colour, supplied[colour]);
    }

    /// <summary>The selected colours in the cartridge's colour order, including independent resource edits.</summary>
    public ImmutableArray<RgbColor> Colours => [.. Enumerable.Range(0, ColourCount)
        .Select(colour => edits.TryGetValue(colour, out RgbColor edited) ? edited : Calculate(colour))];

    /// <summary>The selected background colours, which the Ceres scenes' floors, caption and planet are pictured in.</summary>
    public ImmutableArray<RgbColor> BackgroundColours => Colours[..OwnerColours];

    /// <summary>The selected sprite colours, which the Ceres scenes' actors are pictured in.</summary>
    public ImmutableArray<RgbColor> SpriteColours => Colours[OwnerColours..];

    /// <summary>The colour the Ceres scenes show behind every layer.</summary>
    public RgbColor Backdrop => Colours[BackdropColour];

    /// <summary>
    /// The approach palette: the sprite sets repeat the background sets of the same identity in every visible colour except the
    /// sprites' own set and their own five hue colours; every transparent colour repeats its owner's, and the first
    /// background sets repeat the neutral fill. These are colour relationships, not exemptions for their painted inputs.
    /// </summary>
    private static int Source(int colour)
    {
        bool sprite = colour >= OwnerColours;
        if (colour % SetColours == 0)
            return colour == BackdropColour ? BackdropColour : sprite ? SpriteTransparent : BackgroundTransparent;
        if (sprite && !SpritesOwnColours.Any(region => Within(region, colour)))
            colour -= OwnerColours;
        return Within(NeutralFillColours, colour) ? NeutralFill : colour;
    }

    /// <summary>Whether <paramref name="colour"/> lies in <paramref name="region"/>.</summary>
    private static bool Within((int First, int Count) region, int colour) =>
        colour >= region.First && colour < region.First + region.Count;

    /// <summary>The ramp's six colours between its endpoints interpolate them with nearest-integer rounding.</summary>
    private static bool IsRampInterior(int colour) => colour > RampStart && colour < RampEnd;

    /// <summary>Calculates repeated colours or interpolated ramp colours from independent inputs.</summary>
    private RgbColor Calculate(int colour)
    {
        int source = Source(colour);
        if (!IsRampInterior(source)) return colors[source];
        int step = source - RampStart;
        int steps = RampEnd - RampStart;
        RgbColor first = colors[RampStart], last = colors[RampEnd];
        return first.Zip(last, (_, start, end) => (start * (steps - step) + end * step + steps / 2) / steps);
    }
}
