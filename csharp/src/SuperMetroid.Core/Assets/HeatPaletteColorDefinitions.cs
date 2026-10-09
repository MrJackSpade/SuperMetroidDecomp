using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated repeated-row aliases for the three native Samus heat palettes.</summary>
/// <remarks>Original $8D:E468/E694/E8C0 streams have sixteen fifteen-color frames.
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
    /// <summary>Maps a heat-palette word address to the first equivalent address in its repeated-row progression.</summary>
    /// <param name="pointer">Bank-$8D address of a heat-palette color word.</param>
    /// <param name="canonical">Receives the canonical word address when the pointer is in a supported palette row.</param>
    /// <returns><see langword="true"/> when the address identifies a color in one of the three suit heat programs.</returns>
    internal static bool TryCanonicalPointer(ushort pointer, out ushort canonical)
        => TrySuit(pointer, PaletteFxHeatSuit.Power, out canonical) ||
           TrySuit(pointer, PaletteFxHeatSuit.Varia, out canonical) ||
           TrySuit(pointer, PaletteFxHeatSuit.Gravity, out canonical);

    /// <summary>Resolves a suit's repeated heat-row address to its source row and matching color slot.</summary>
    /// <param name="pointer">Candidate color-word address in a heat-palette program.</param>
    /// <param name="suit">Power, Varia, or Gravity program whose row layout is being examined.</param>
    /// <param name="canonical">Receives the corresponding source word address, or zero when the pointer is not a color word.</param>
    /// <returns><see langword="true"/> when the pointer falls on an aligned color word in the suit's animated records.</returns>
    private static bool TrySuit(ushort pointer, PaletteFxHeatSuit suit, out ushort canonical)
    {
        int first = PaletteFxHeatInstructionListDefinitions.Resolve(suit, 0) + 2;
        int offset = pointer - first;
        if ((uint)offset >= 16 * 34 || (offset & 1) != 0 || offset % 34 >= 30) { canonical = 0; return false; }
        int phase = offset / 34;
        int row = phase == 15 ? (suit == PaletteFxHeatSuit.Power ? 1 : 0) :
            (Math.Min(phase, 15 - phase) + 1) / 2;
        int firstPhase = row == 0 ? 0 : 2 * row - 1;
        int color = offset % 34 / 2;
        bool sharedWithPower = suit != PaletteFxHeatSuit.Power && color is not (1 or 9 or 10) &&
            !(suit == PaletteFxHeatSuit.Gravity && row == 0 && color == 11);
        if (sharedWithPower) first = PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Power, 0) + 2;
        if (color == 5 || (suit != PaletteFxHeatSuit.Power && color == 1)) firstPhase = 0;
        canonical = (ushort)(first + firstPhase * 34 + offset % 34);
        return true;

    }
    /// <summary>Attempts each supported alias and interpolation rule to derive a heat-palette color.</summary>
    /// <param name="pointer">Bank-$8D address of the candidate color word.</param>
    /// <param name="colors">Installed color inputs and explicit overrides used by the calculation rules.</param>
    /// <param name="value">Receives the calculated packed color when a rule recognizes the address.</param>
    /// <returns><see langword="true"/> when a supported base-color, endpoint, or heat-ramp rule supplies the word.</returns>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        TryBaseColor(pointer, colors, out value) || TryHighlightEndpoint(pointer, colors, out value) || TryRedRamp(pointer, colors, out value) || TrySecondaryRedRamp(pointer, colors, out value) ||
        TryMixedRamp(pointer, colors, out value) || TrySharedRed(pointer, colors, out value);

    /// <summary>Maps heat row-zero colors to the corresponding normal suit-loading colors.</summary>
    /// <remarks>The fifteen colors at $8D:E468/E694/E8C0 equal colors1..15 in the
    /// initial loading palettes at $8D:DB6B/DCD1/DE37, and independently equal normal
    /// suit palettes $9B:9402/9522/9802. Heat omits the transparent index zero.
    /// Native program layout and every original color were independently checked.
    /// This aliases identical suit artwork; it does not classify the source artwork
    /// as irreducible. Differing heat edits retain their own installed value.</remarks>
    internal static bool TryBasePalettePointer(ushort pointer, out ushort source)
    {
        int first;
        int loading;
        if (pointer >= 0xe468 && pointer < 0xe486) { first = 0xe468; loading = 0xdb6d; }
        else if (pointer >= 0xe694 && pointer < 0xe6b2) { first = 0xe694; loading = 0xdcd3; }
        else if (pointer >= 0xe8c0 && pointer < 0xe8de) { first = 0xe8c0; loading = 0xde39; }
        else { source = 0; return false; }
        if (((pointer - first) & 1) != 0) { source = 0; return false; }
        source = (ushort)(loading + pointer - first);
        return true;
    }

    /// <summary>Resolves a heat row-zero color through its matching normal suit-loading palette entry.</summary>
    /// <param name="pointer">Heat-palette word address to resolve.</param>
    /// <param name="colors">Installed palette words used by the loading-palette resolver.</param>
    /// <param name="value">Receives the matching packed base color when the address has a known alias.</param>
    /// <returns><see langword="true"/> when the word maps to an available normal loading color.</returns>
    private static bool TryBaseColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        return TryBasePalettePointer(pointer, out ushort source) &&
            LoadingPaletteColorDefinitions.TryReadColor(source, colors, out value);
    }

    /// <summary>Gets an endpoint input from an explicit edit, its base-palette alias, or a derived highlight.</summary>
    /// <param name="pointer">Heat-palette endpoint address whose color is needed by a calculation.</param>
    /// <param name="colors">Installed color values, including any explicit endpoint edits.</param>
    /// <param name="value">Receives the first matching explicit or derived endpoint color.</param>
    /// <returns><see langword="true"/> when one of the supported sources provides the endpoint value.</returns>
    private static bool TryInputColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        colors.TryGetValue(pointer, out value) || TryBaseColor(pointer, colors, out value) ||
        TryHighlightEndpoint(pointer, colors, out value);

    /// <summary>Bright red highlights move halfway toward saturated red, rounding upward.</summary>
    /// <remarks>Original Power slots1/8 rise from29/27 to30/29; Varia slot9 rises
    /// from30 to31. Each endpoint is ceil((baseRed+31)/2), preserving green/blue.
    /// Sources: $8D:E558/E566/E794 and their corresponding row-zero colors. This
    /// shared highlight rule uses no stored endpoint or fitted per-slot coefficients.
    /// Player endpoints differing from the result remain explicit installed overrides.</remarks>
    internal static bool TryHighlightEndpoint(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        ushort source = pointer switch
        {
            0xe558 => 0xe46a,
            0xe566 => 0xe478,
            0xe794 => 0xe6a6,
            _ => 0,
        };
        value = 0;
        if (source == 0 || !(colors.TryGetValue(source, out ushort start) || TryBaseColor(source, colors, out start))) return false;
        value = (ushort)((start & 0x7fe0) | (((start & 31) + 32) / 2));
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
    internal static bool TryRedRamp(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
        => TryEndpointRed(pointer, colors, PaletteFxHeatSuit.Power, 3, 0, out value);

    /// <summary>Calculates the remaining single-channel red gradients from their endpoints.</summary>
    /// <remarks>Original Power slot1 reds29,29,30,30,30 and slot8 reds27,28,28,29,29
    /// are nearest-rounded quarter steps, ties upward. Varia slot9 reds30,30,30,30,31
    /// are floor-rounded quarter steps. All retain initial green/blue. Supported
    /// NTSC J/U v1.0 words at $8D:E46A/E478/E6A6 plus the native phase strides
    /// independently establish all five samples. These exact conventional interpolation
    /// rules make no claim about the historical authoring software. Endpoints remain
    /// editable inputs; differing intermediate colors remain explicit overrides.</remarks>
    internal static bool TrySecondaryRedRamp(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        TryEndpointRed(pointer, colors, PaletteFxHeatSuit.Power, 1, 2, out value) ||
        TryEndpointRed(pointer, colors, PaletteFxHeatSuit.Power, 8, 2, out value) ||
        TryEndpointRed(pointer, colors, PaletteFxHeatSuit.Varia, 9, 0, out value);

    /// <summary>Calculates an interior red-ramp sample while retaining the starting color's green and blue channels.</summary>
    /// <param name="pointer">Candidate address of an interior palette sample.</param>
    /// <param name="colors">Installed endpoint values used to interpolate the red component.</param>
    /// <param name="suit">Suit program containing the ramp's source color.</param>
    /// <param name="colorIndex">Index of the ramp color within each timed palette record.</param>
    /// <param name="roundingBias">Integer bias applied before dividing the weighted red sum by four.</param>
    /// <param name="value">Receives the interpolated color when the address is one of the three interior samples.</param>
    /// <returns><see langword="true"/> when the pointer identifies a supported interior sample and both endpoints resolve.</returns>
    private static bool TryEndpointRed(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors,
        PaletteFxHeatSuit suit, int colorIndex, int roundingBias, out ushort value)
    {
        value = 0;
        int first = PaletteFxHeatInstructionListDefinitions.Resolve(suit, 0) + 2 + 2 * colorIndex;
        int offset = pointer - first;
        if (offset is not (34 or 102 or 170)) return false;
        if (!TryInputColor((ushort)first, colors, out ushort start) ||
            !TryInputColor((ushort)(first + 7 * 34), colors, out ushort end)) return false;
        int row = (offset / 34 + 1) / 2;
        int red = ((start & 31) * (4 - row) + (end & 31) * row + roundingBias) / 4;
        value = (ushort)((start & 0x7fe0) | red);
        return true;
    }

    /// <summary>Calculates the shared slot4 gradient's interior RGB colors.</summary>
    /// <remarks>Original rows at $8D:E470 plus phases0,1,3,5,7 have red8,10,12,14,15;
    /// green13,14,14,15,16; blue8,9,9,10,11. Red is upward-rounded endpoint
    /// interpolation. Green is nearest-even endpoint interpolation; blue shares its
    /// change, preserving the initial green/blue difference. Each rule matches the
    /// complete original channel independently. This exact bounded representation
    /// does not assert historical tooling. Edited blue results outside RGB5 decline
    /// calculation so their explicit supplied values survive; no clamping is added.</remarks>
    internal static bool TryMixedRamp(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        int first = PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Power, 0) + 10;
        int offset = pointer - first;
        if (offset is not (34 or 102 or 170)) return false;
        if (!TryInputColor((ushort)first, colors, out ushort start) ||
            !TryInputColor((ushort)(first + 7 * 34), colors, out ushort end)) return false;
        int row = (offset / 34 + 1) / 2;
        int red = ((start & 31) * (4 - row) + (end & 31) * row + 3) / 4;
        int initialGreen = start >> 5 & 31;
        int greenSum = initialGreen * (4 - row) + (end >> 5 & 31) * row;
        int green = greenSum / 4;
        int remainder = greenSum % 4;
        if (remainder > 2 || (remainder == 2 && (green & 1) != 0)) green++;
        int blue = (start >> 10 & 31) + green - initialGreen;
        if ((uint)blue > 31) return false;
        value = (ushort)(red | green << 5 | blue << 10);
        return true;
    }

    /// <summary>Reconstructs shared red heating while preserving the base green/blue.</summary>
    /// <remarks>Across all five original rows, Power slots0,2,6,7,9..14,
    /// Varia slot10 and Gravity slots9,10 add the same red delta as Power slot3.
    /// Base colors and slot3 endpoints remain independently owned inputs. Interior
    /// slot3 samples are calculated by TryRedRamp when no edited override exists. Only canonical
    /// noninitial rows qualify. Nonrepresentable edited deltas return false so the
    /// loader keeps the supplied independent RGB5 color instead of clamping it.</remarks>
    internal static bool TrySharedRed(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        if (!TryCanonicalPointer(pointer, out ushort canonical) || pointer != canonical) return false;
        int powerFirst = PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Power, 0) + 2;
        int first;
        int index;
        if (pointer >= powerFirst && pointer < powerFirst + 16 * 34)
        {
            first = powerFirst;
            index = (pointer - first) % 34 / 2;
            if (index is not (0 or 2 or 6 or 7 or 9 or 10 or 11 or 12 or 13 or 14)) return false;
        }
        else
        {
            int variaFirst = PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Varia, 0) + 2;
            first = pointer < variaFirst + 16 * 34 ? variaFirst :
                PaletteFxHeatInstructionListDefinitions.Resolve(PaletteFxHeatSuit.Gravity, 0) + 2;
            index = (pointer - first) % 34 / 2;
            if (first == variaFirst ? index != 10 : index is not (9 or 10)) return false;
        }
        int phase = (pointer - first) / 34;
        if (phase == 0) return false;
        if (!TryInputColor((ushort)(first + index * 2), colors, out ushort original) ||
            !TryInputColor((ushort)(powerFirst + 6), colors, out ushort baseline) ||
            !(colors.TryGetValue((ushort)(powerFirst + phase * 34 + 6), out ushort heated) ||
              TryRedRamp((ushort)(powerFirst + phase * 34 + 6), colors, out heated))) return false;
        int red = (original & 31) + (heated & 31) - (baseline & 31);
        if ((uint)red > 31) return false;
        value = (ushort)((original & 0x7fe0) | red);
        return true;
    }}
