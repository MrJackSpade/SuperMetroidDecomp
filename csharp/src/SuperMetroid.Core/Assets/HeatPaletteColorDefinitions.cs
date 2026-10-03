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
/// These original columns are unchanged across the complete animation. Other distinct colors need separate
/// review. Custom colors differing from the calculated alias remain explicit overrides.</remarks>
internal static class HeatPaletteColorDefinitions
{
    internal static bool TryCanonicalPointer(ushort pointer, out ushort canonical)
        => TrySuit(pointer, PaletteFxHeatSuit.Power, out canonical) ||
           TrySuit(pointer, PaletteFxHeatSuit.Varia, out canonical) ||
           TrySuit(pointer, PaletteFxHeatSuit.Gravity, out canonical);

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
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value) =>
        TryRedRamp(pointer, colors, out value) || TrySecondaryRedRamp(pointer, colors, out value) ||
        TrySharedRed(pointer, colors, out value);

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

    private static bool TryEndpointRed(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors,
        PaletteFxHeatSuit suit, int colorIndex, int roundingBias, out ushort value)
    {
        value = 0;
        int first = PaletteFxHeatInstructionListDefinitions.Resolve(suit, 0) + 2 + 2 * colorIndex;
        int offset = pointer - first;
        if (offset is not (34 or 102 or 170)) return false;
        if (!colors.TryGetValue((ushort)first, out ushort start) ||
            !colors.TryGetValue((ushort)(first + 7 * 34), out ushort end)) return false;
        int row = (offset / 34 + 1) / 2;
        int red = ((start & 31) * (4 - row) + (end & 31) * row + roundingBias) / 4;
        value = (ushort)((start & 0x7fe0) | red);
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
        if (!colors.TryGetValue((ushort)(first + index * 2), out ushort original) ||
            !colors.TryGetValue((ushort)(powerFirst + 6), out ushort baseline) ||
            !(colors.TryGetValue((ushort)(powerFirst + phase * 34 + 6), out ushort heated) ||
              TryRedRamp((ushort)(powerFirst + phase * 34 + 6), colors, out heated))) return false;
        int red = (original & 31) + (heated & 31) - (baseline & 31);
        if ((uint)red > 31) return false;
        value = (ushort)((original & 0x7fe0) | red);
        return true;
    }}
