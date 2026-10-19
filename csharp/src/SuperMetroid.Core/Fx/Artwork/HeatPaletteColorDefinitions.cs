using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Calculated repeated-row aliases for the three native Samus heat palettes.</summary>
/// <remarks>Each suit's program has sixteen fifteen-color phases.
/// Interior phase p selects row (min(p,15-p)+1)/2. Row zero first occurs at phase0;
/// row r=1..4 first occurs at phase2*r-1. Phase15 returns to row1 for Power and
/// row0 for protected suits. Full original rows independently agree with this mirrored
/// progression. Varia and Gravity share Power's corresponding color slots except
/// suit-specific slots1,9,10 and Gravity's row-zero slot11. These are equal inks at
/// the same palette indices, not a permutation or a fitted color transform. This
/// also selects row zero for the constant slot5 in every suit and slot1 in Varia/Gravity.
/// These original columns are unchanged across the complete animation. Endpoint component
/// ownership is documented by HeatPaletteInputView; underlying suit artwork has its own
/// review. Custom colors differing from the calculated alias remain explicit overrides.</remarks>
internal static class HeatPaletteColorDefinitions
{
    /// <summary>Power phase zero, color three: the base heat color supplying <see cref="HeatPaletteEndpoint.Red"/>'s unselected channels.</summary>
    internal static PaletteFxColorKey RedEndpointBase => Rows(PaletteFxHeatSuit.Power).Color(0, 3);

    /// <summary>Power phase zero, color four: the base heat color supplying <see cref="HeatPaletteEndpoint.Mixed"/>'s blue.</summary>
    internal static PaletteFxColorKey MixedEndpointBase => Rows(PaletteFxHeatSuit.Power).Color(0, 4);

    /// <summary>The heat palette color <paramref name="endpoint"/> names.</summary>
    internal static PaletteFxColorKey Key(HeatPaletteEndpoint endpoint) => endpoint switch
    {
        HeatPaletteEndpoint.Red => Rows(PaletteFxHeatSuit.Power).Color(7, 3),
        HeatPaletteEndpoint.Mixed => Rows(PaletteFxHeatSuit.Power).Color(7, 4),
        _ => throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, "Not a heat palette endpoint."),
    };

    /// <summary>The sixteen phases of <paramref name="suit"/>'s heat program.</summary>
    internal static PaletteFxColorRows Rows(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => PaletteFxHeatProgramMechanicsDefinitions.All[0].Rows,
        PaletteFxHeatSuit.Varia => PaletteFxHeatProgramMechanicsDefinitions.All[1].Rows,
        PaletteFxHeatSuit.Gravity => PaletteFxHeatProgramMechanicsDefinitions.All[2].Rows,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>Maps repeated heat-frame colors in any suit to their first equivalent paint row.</summary>
    internal static bool TryCanonical(PaletteFxColorKey key, out PaletteFxColorKey canonical)
        => TrySuit(key, PaletteFxHeatSuit.Power, out canonical) ||
           TrySuit(key, PaletteFxHeatSuit.Varia, out canonical) ||
           TrySuit(key, PaletteFxHeatSuit.Gravity, out canonical);

    /// <summary>Canonicalizes one suit's mirrored phases, shared Power slots, and constant color columns.</summary>
    private static bool TrySuit(PaletteFxColorKey key, PaletteFxHeatSuit suit, out PaletteFxColorKey canonical)
    {
        PaletteFxColorRows rows = Rows(suit);
        if (!rows.TryLocate(key, out int phase, out int color)) { canonical = default; return false; }
        int row = phase == 15 ? (suit == PaletteFxHeatSuit.Power ? 1 : 0) :
            (Math.Min(phase, 15 - phase) + 1) / 2;
        int firstPhase = row == 0 ? 0 : 2 * row - 1;
        bool sharedWithPower = suit != PaletteFxHeatSuit.Power && color is not (1 or 9 or 10) &&
            !(suit == PaletteFxHeatSuit.Gravity && row == 0 && color == 11);
        if (sharedWithPower) rows = Rows(PaletteFxHeatSuit.Power);
        if (color == 5 || (suit != PaletteFxHeatSuit.Power && color == 1)) firstPhase = 0;
        canonical = rows.Color(firstPhase, color);
        return true;
    }

    /// <summary>Tries loading-palette aliases and proven heating ramps before retaining supplied paint.</summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value) =>
        TryBaseColor(key, colors, out value) || TryHighlightEndpoint(key, colors, out value) || TryRedRamp(key, colors, out value) || TrySecondaryRedRamp(key, colors, out value) ||
        TryMixedRamp(key, colors, out value) || TrySharedRed(key, colors, out value);

    /// <summary>Maps heat row-zero colors to the corresponding normal suit-loading colors.</summary>
    /// <remarks>Each suit's phase-zero colors 0..14 equal colors 1..15 of the initial row of the same suit's
    /// loading palette, and independently equal normal suit palettes $9B:9402/9522/9802. Heat omits the
    /// transparent index zero. Native program layout and every original color were independently checked.
    /// This aliases identical suit artwork; it does not classify the source artwork as irreducible. Differing
    /// heat edits retain their own installed value.</remarks>
    internal static bool TryBasePalette(PaletteFxColorKey key, out PaletteFxColorKey source)
    {
        foreach (PaletteFxHeatSuit suit in (ReadOnlySpan<PaletteFxHeatSuit>)[PaletteFxHeatSuit.Power, PaletteFxHeatSuit.Varia, PaletteFxHeatSuit.Gravity])
        {
            if (Rows(suit).TryLocate(key, out int phase, out int color) && phase == 0)
            {
                source = LoadingPaletteColorDefinitions.Rows(suit).Color(0, color + 1);
                return true;
            }
        }
        source = default;
        return false;
    }

    /// <summary>Resolves heat row zero through the matching initial suit-loading palette color.</summary>
    private static bool TryBaseColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        return TryBasePalette(key, out PaletteFxColorKey source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value);
    }

    /// <summary>Prefers a supplied color, then tries a base-palette alias or calculated highlight endpoint.</summary>
    private static bool TryInputColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value) =>
        colors.TryGetValue(key, out value) || TryBaseColor(key, colors, out value) ||
        TryHighlightEndpoint(key, colors, out value);

    /// <summary>Bright red highlights move halfway toward saturated red, rounding upward.</summary>
    /// <remarks>Original Power slots1/8 rise from29/27 to30/29; Varia slot9 rises
    /// from30 to31. Each endpoint is ceil((baseRed+31)/2), preserving green/blue.
    /// Sources: phase seven of Power colors 1 and 8 and Varia color 9, from the same colors at phase zero. This
    /// shared highlight rule uses no stored endpoint or fitted per-slot coefficients.
    /// Player endpoints differing from the result remain explicit installed overrides.</remarks>
    internal static bool TryHighlightEndpoint(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        PaletteFxHeatSuit suit;
        if (Rows(PaletteFxHeatSuit.Power).TryLocate(key, out int phase, out int color) && phase == 7 && color is 1 or 8)
            suit = PaletteFxHeatSuit.Power;
        else if (Rows(PaletteFxHeatSuit.Varia).TryLocate(key, out phase, out color) && phase == 7 && color == 9)
            suit = PaletteFxHeatSuit.Varia;
        else
            return false;
        PaletteFxColorKey source = Rows(suit).Color(0, color);
        if (!(colors.TryGetValue(source, out Bgr555 start) || TryBaseColor(source, colors, out start))) return false;
        value = start.WithRed((start.Red + 32) / 2);
        return true;
    }

    /// <summary>Calculates the three interior samples of the shared five-level red ramp.</summary>
    /// <remarks>Power slot3's original red levels0,1,2,3,5 are the floor-rounded
    /// quarter steps from0 to5. Its green31/blue14 remain fixed. Thirteen other
    /// color tracks share this red difference, independently verified from the native
    /// rows. Keep the endpoint colors as inputs; interior row r=1..3 is
    /// floor((startRed*(4-r)+endRed*r)/4), with the initial green/blue. Integer
    /// weighted sums also give defined, bounded RGB5 results for edited endpoints.
    /// This is the sampled linear heating gradient, not a fit with corrections.</remarks>
    internal static bool TryRedRamp(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value)
        => TryEndpointRed(key, colors, PaletteFxHeatSuit.Power, 3, 0, out value);

    /// <summary>Calculates the remaining single-channel red gradients from their endpoints.</summary>
    /// <remarks>Original Power slot1 reds29,29,30,30,30 and slot8 reds27,28,28,29,29
    /// are nearest-rounded quarter steps, ties upward. Varia slot9 reds30,30,30,30,31
    /// are floor-rounded quarter steps. All retain initial green/blue. Supported
    /// NTSC J/U v1.0 phase-zero words of those colors plus the native phase rows
    /// independently establish all five samples. These exact conventional interpolation
    /// rules make no claim about the historical authoring software. Endpoints remain
    /// editable inputs; differing intermediate colors remain explicit overrides.</remarks>
    internal static bool TrySecondaryRedRamp(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value) =>
        TryEndpointRed(key, colors, PaletteFxHeatSuit.Power, 1, 2, out value) ||
        TryEndpointRed(key, colors, PaletteFxHeatSuit.Power, 8, 2, out value) ||
        TryEndpointRed(key, colors, PaletteFxHeatSuit.Varia, 9, 0, out value);

    /// <summary>Interpolates an interior red level between editable endpoints using the selected rounding bias.</summary>
    private static bool TryEndpointRed(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors,
        PaletteFxHeatSuit suit, int colorIndex, int roundingBias, out Bgr555 value)
    {
        value = Bgr555.Black;
        PaletteFxColorRows rows = Rows(suit);
        if (!rows.TryLocate(key, out int phase, out int color) || color != colorIndex || phase is not (1 or 3 or 5)) return false;
        if (!TryInputColor(rows.Color(0, colorIndex), colors, out Bgr555 start) ||
            !TryInputColor(rows.Color(7, colorIndex), colors, out Bgr555 end)) return false;
        int row = (phase + 1) / 2;
        int red = (start.Red * (4 - row) + end.Red * row + roundingBias) / 4;
        value = start.WithRed(red);
        return true;
    }

    /// <summary>Calculates the shared slot4 gradient's interior RGB colors.</summary>
    /// <remarks>Original Power color 4 at phases0,1,3,5,7 has red8,10,12,14,15;
    /// green13,14,14,15,16; blue8,9,9,10,11. Red is upward-rounded endpoint
    /// interpolation. Green is nearest-even endpoint interpolation; blue shares its
    /// change, preserving the initial green/blue difference. Each rule matches the
    /// complete original channel independently. This exact bounded representation
    /// does not assert historical tooling. Edited blue results outside RGB5 decline
    /// calculation so their explicit supplied values survive; no clamping is added.</remarks>
    internal static bool TryMixedRamp(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        PaletteFxColorRows rows = Rows(PaletteFxHeatSuit.Power);
        if (!rows.TryLocate(key, out int phase, out int color) || color != 4 || phase is not (1 or 3 or 5)) return false;
        if (!TryInputColor(rows.Color(0, 4), colors, out Bgr555 start) ||
            !TryInputColor(rows.Color(7, 4), colors, out Bgr555 end)) return false;
        int row = (phase + 1) / 2;
        int red = (start.Red * (4 - row) + end.Red * row + 3) / 4;
        int initialGreen = start.Green;
        int greenSum = initialGreen * (4 - row) + end.Green * row;
        int green = greenSum / 4;
        int remainder = greenSum % 4;
        if (remainder > 2 || (remainder == 2 && (green & 1) != 0)) green++;
        int blue = start.Blue + green - initialGreen;
        if ((uint)blue > 31) return false;
        value = new(red, green, blue);
        return true;
    }

    /// <summary>Reconstructs shared red heating while preserving the base green/blue.</summary>
    /// <remarks>Across all five original rows, Power slots0,2,6,7,9..14,
    /// Varia slot10 and Gravity slots9,10 add the same red delta as Power slot3.
    /// Base colors and slot3 endpoints remain independently owned inputs. Interior
    /// slot3 samples are calculated by TryRedRamp when no edited override exists. Only canonical
    /// noninitial rows qualify. Nonrepresentable edited deltas return false so the
    /// loader keeps the supplied independent RGB5 color instead of clamping it.</remarks>
    internal static bool TrySharedRed(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        if (!TryCanonical(key, out PaletteFxColorKey canonical) || key != canonical) return false;
        PaletteFxColorRows power = Rows(PaletteFxHeatSuit.Power);
        PaletteFxColorRows rows;
        int phase, index;
        if (power.TryLocate(key, out phase, out index))
        {
            rows = power;
            if (index is not (0 or 2 or 6 or 7 or 9 or 10 or 11 or 12 or 13 or 14)) return false;
        }
        else if (Rows(PaletteFxHeatSuit.Varia).TryLocate(key, out phase, out index))
        {
            rows = Rows(PaletteFxHeatSuit.Varia);
            if (index != 10) return false;
        }
        else
        {
            rows = Rows(PaletteFxHeatSuit.Gravity);
            if (!rows.TryLocate(key, out phase, out index) || index is not (9 or 10)) return false;
        }
        if (phase == 0) return false;
        if (!TryInputColor(rows.Color(0, index), colors, out Bgr555 original) ||
            !TryInputColor(power.Color(0, 3), colors, out Bgr555 baseline) ||
            !(colors.TryGetValue(power.Color(phase, 3), out Bgr555 heated) ||
              TryRedRamp(power.Color(phase, 3), colors, out heated))) return false;
        int red = original.Red + heated.Red - baseline.Red;
        if ((uint)red > 31) return false;
        value = original.WithRed(red);
        return true;
    }
}
