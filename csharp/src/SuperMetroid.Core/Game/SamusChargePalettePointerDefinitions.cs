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

    /// <summary>Resolves $91:D7D5's suit-specific charge list as shade sequence 0/1/2/3/2/1, retaining its bank-$9B palette identity without reading colors or advancing the charge cycle.</summary>
    /// <param name="suitOffset">Even byte offset 0, 2, or 4 into the Power, Varia, or Gravity suit pointer table; not an unscaled suit ordinal.</param>
    /// <param name="phaseOffset">Even byte offset 0..10 into the six-word phase list.</param>
    /// <param name="pointer">Native bank-$9B sixteen-color palette word pointer, or zero for an unsupported suit/phase offset.</param>
    /// <returns>True for the bounded suit and phase domain; invalid offsets return false rather than selecting adjacent ROM words.</returns>
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

    /// <summary>Resolves $91:D7FF's pseudo-screw list: the first three phases use the suit's shinespark/pseudo-screw shade three, and the last three restore its normal palette; timing remains caller-owned.</summary>
    /// <param name="suitOffset">Even native suit-table byte offset 0 (Power), 2 (Varia), or 4 (Gravity).</param>
    /// <param name="phaseOffset">Even byte offset 0..10; offsets 0/2/4 choose the bright shade and 6/8/10 choose normal colors.</param>
    /// <param name="pointer">Native bank-$9B sixteen-color palette word pointer, or zero when either offset is unsupported.</param>
    /// <returns>True only for a compiled suit/phase selection; this lookup neither applies colors nor advances animation.</returns>
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

    /// <summary>Resolves $91:D829's reversed Hyper-shot glow pointer table, skipping its padding zero: offsets $02..$14 select frames nine down to zero in $20-byte palette strides.</summary>
    /// <param name="tableOffset">Even byte offset 2..20 from the native pointer-table start, not a frame ordinal or palette-data byte offset.</param>
    /// <param name="pointer">Native bank-$9B sixteen-color palette word pointer, or zero for a padding, odd, or out-of-range offset.</param>
    /// <returns>True for one of the ten authored glow frames; colors remain installed presentation data and glow countdown remains caller-owned.</returns>
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
