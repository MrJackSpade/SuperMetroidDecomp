namespace SuperMetroid.Core.Game;

/// <summary>Bounded bank-$91 pointer selection for Samus's charge and Hyper-shot colors.</summary>
/// <remarks>
/// These relationships are verified against every top-level and nested pointer in the
/// pinned cartridge. The target RGB5 words remain authored presentation data.
/// </remarks>
public static class SamusChargePalettePointerDefinitions
{
    /// <summary>Bank-$9B first Power Suit charged-beam shade, selected by $91:D7D5.</summary>
    public const ushort FirstChargedBeamPalette = 0x9820;
    /// <summary>Bank-$9B first Hyper-shot glow frame, selected by $91:D829.</summary>
    public const ushort FirstHyperShotPalette = 0xa240;
    /// <summary>Byte stride between adjacent sixteen-color palettes.</summary>
    public const int ColorPaletteByteStride = 0x20;
    /// <summary>Byte stride between charged-beam suit families.</summary>
    public const int ChargedSuitByteStride = 0x100;
    /// <summary>Highest native byte offset into either six-word charge list.</summary>
    public const ushort LastChargePhaseByteOffset = 10;
    /// <summary>First native byte offset into the reversed Hyper-shot pointer table.</summary>
    public const ushort FirstHyperTableByteOffset = 2;
    /// <summary>Last native byte offset into the reversed Hyper-shot pointer table.</summary>
    public const ushort LastHyperTableByteOffset = 0x14;

    public static bool TryChargedBeam(ushort suitOffset, ushort phaseOffset,
        out ushort pointer)
    {
        if (!ValidSuit(suitOffset) || !ValidChargePhase(phaseOffset))
        {
            pointer = 0;
            return false;
        }
        int phase = phaseOffset / sizeof(ushort);
        int shade = Math.Min(phase, 6 - phase);
        pointer = (ushort)(FirstChargedBeamPalette +
            suitOffset / sizeof(ushort) * ChargedSuitByteStride +
            shade * ColorPaletteByteStride);
        return true;
    }

    public static bool TryPseudoScrew(ushort suitOffset, ushort phaseOffset,
        out ushort pointer)
    {
        if (!ValidSuit(suitOffset) || !ValidChargePhase(phaseOffset))
        {
            pointer = 0;
            return false;
        }
        int phase = phaseOffset / sizeof(ushort);
        if (phase < 3)
            return SamusPaletteRomData.FullBodyCycles.TryActiveShinesparkPalettePointer(
                suitOffset, 6, out pointer);
        pointer = SamusPaletteRomData.Common.NormalSuitPalettePointer(suitOffset);
        return true;
    }

    public static bool TryHyperShot(ushort tableOffset, out ushort pointer)
    {
        if (tableOffset < FirstHyperTableByteOffset ||
            tableOffset > LastHyperTableByteOffset || (tableOffset & 1) != 0)
        {
            pointer = 0;
            return false;
        }
        int frame = (LastHyperTableByteOffset - tableOffset) / sizeof(ushort);
        pointer = (ushort)(FirstHyperShotPalette + frame * ColorPaletteByteStride);
        return true;
    }

    private static bool ValidSuit(ushort offset) => offset <= 4 && (offset & 1) == 0;
    private static bool ValidChargePhase(ushort offset) =>
        offset <= LastChargePhaseByteOffset && (offset & 1) == 0;
}
