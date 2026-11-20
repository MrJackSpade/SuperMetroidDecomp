using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shades of the palette-FX colour groups that recolour Samus's suit in gameplay (#1512): her heat glow in a heated
/// room and her suit materializing as she loads, which the import pre-colours every one of her sheets in. Each group starts
/// at one colour of her suit and runs to the suit's last, as the palette-FX handler writes a record over her suit's
/// colours; she lays the record's shade over her suit through her colour state.
/// </summary>
internal static class PaletteFxSamusShades
{
    /// <summary>The heat pulses' suits in the order Samus's sheets list their shades (#2567).</summary>
    private static readonly IReadOnlyList<ColourEffectHeatSuit> HeatSuitOrder = Ensure.EveryMemberOnce<ColourEffectHeatSuit>(
    [
        ColourEffectHeatSuit.Power,
        ColourEffectHeatSuit.Varia,
        ColourEffectHeatSuit.Gravity,
    ]);

    /// <summary>The first colour of her suit each of her suit's groups recolours.</summary>
    private static readonly ImmutableDictionary<ColourEffectColours, int> FirstSuitColours = new Dictionary<ColourEffectColours, int>
    {
        [ColourEffectColours.HeatSuitGlow] = 1,
        [ColourEffectColours.SamusLoadingSuit] = 0,
    }.ToImmutableDictionary();

    /// <summary>
    /// The objects whose programs recolour her suit: each suit's heat pulse, which her heat shimmer shows (#1512, #1598,
    /// #2366), and her three suits' loading.
    /// </summary>
    private static readonly ImmutableArray<ColourEffectId> SuitObjects =
    [
        .. HeatSuitOrder.Select(ColourEffectHeatAnimations.Pulse),
        ColourEffectId.SamusLoadingPowerSuit, ColourEffectId.SamusLoadingVariaSuit, ColourEffectId.SamusLoadingGravitySuit,
    ];

    /// <summary>Every record of her suit's groups, shaded from <paramref name="colours"/> over her sheets' suit inks.</summary>
    /// <exception cref="InvalidDataException">A record has no colour in the import's palette-effect colours, or writes past her suit's colours.</exception>
    internal static IReadOnlyList<SheetShade> Of(IPaletteFxColorSource colours)
    {
        Ensure.NotNull(colours);
        ShownColourEffectFrame[] shown = PaletteFxRecordShades.ShownBy(SuitObjects);
        return [.. shown.Where(entry => FirstSuitColours.ContainsKey(entry.Colours))
            .Select(entry => PaletteFxRecordShades.Shade(entry.Colours, entry.Frame,
                new InkRange(SamusSheetInks.Suit + FirstSuitColours[entry.Colours], SamusSheetInks.Suit + SamusSheetInks.GroupColours), colours))];
    }
}
