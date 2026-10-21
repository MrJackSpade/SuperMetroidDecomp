namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The bounded bank-$91 suit and phase domains of Samus's charge and Hyper-shot palette lists.</summary>
/// <remarks>
/// These domains are verified against every top-level and nested list entry in the pinned cartridge. The colors they
/// select remain authored presentation data.
/// </remarks>
public static class SamusChargePaletteDefinitions
{
    /// <summary>Highest native byte offset into either six-word charge list.</summary>
    public const ushort LastChargePhaseByteOffset = 10;
    /// <summary>First native byte offset into the reversed Hyper-shot list.</summary>
    public const ushort FirstHyperTableByteOffset = 2;
    /// <summary>Last native byte offset into the reversed Hyper-shot list.</summary>
    public const ushort LastHyperTableByteOffset = 0x14;

    /// <summary>
    /// Whether $91:D7D5's charged-beam lists and $91:D7FF's pseudo-screw lists hold a palette for the suit and phase:
    /// the suit table's three even offsets and the six-phase list's six.
    /// </summary>
    /// <param name="suitOffset">Even byte offset 0, 2, or 4 into the Power, Varia, or Gravity suit table; not an unscaled suit ordinal.</param>
    /// <param name="phaseOffset">Even byte offset 0..10 into the six-word phase list.</param>
    public static bool IsCompiledChargePhase(ushort suitOffset, ushort phaseOffset) =>
        ValidSuit(suitOffset) && ValidChargePhase(phaseOffset);

    /// <summary>Whether $91:D829's reversed Hyper-shot glow list holds a frame at the byte offset, skipping its padding zero.</summary>
    /// <param name="tableOffset">Even byte offset 2..20 from the native list start, not a frame ordinal or palette-data byte offset.</param>
    public static bool IsCompiledHyperShot(ushort tableOffset) =>
        tableOffset >= FirstHyperTableByteOffset && tableOffset <= LastHyperTableByteOffset && (tableOffset & 1) == 0;

    /// <summary>Accepts only the three even byte offsets of the native suit pointer table.</summary>
    private static bool ValidSuit(ushort offset) => offset <= 4 && (offset & 1) == 0;
    /// <summary>Accepts only six word-aligned byte offsets in the charge phase list.</summary>
    private static bool ValidChargePhase(ushort offset) =>
        offset <= LastChargePhaseByteOffset && (offset & 1) == 0;
}
