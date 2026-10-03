using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated repeated-row aliases for the three native Samus heat palettes.</summary>
/// <remarks>Original $8D:E468/E694/E8C0 streams have sixteen fifteen-color frames.
/// Interior phase p selects row (min(p,15-p)+1)/2. Row zero first occurs at phase0;
/// row r=1..4 first occurs at phase2*r-1. Phase15 returns to row1 for Power and
/// row0 for protected suits. Full original rows independently agree with this mirrored
/// progression. This resolves repetition only; distinct row colors remain a separate
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
            canonical = (ushort)(first + firstPhase * 34 + offset % 34);
            return true;

    }
}
