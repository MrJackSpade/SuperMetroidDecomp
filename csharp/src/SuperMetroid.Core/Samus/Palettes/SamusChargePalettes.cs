using static SuperMetroid.Core.Samus.Palettes.SamusChargePaletteDefinitions;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which charge phases, hyper shots and suits the compiled charge palettes cover.</summary>
public static class SamusChargePalettes
{
    /// <summary>
    /// Whether $91:D7D5's charged-beam lists and $91:D7FF's pseudo-screw lists hold a palette for the suit and phase:
    /// the suit table's three even offsets and the six-phase list's six.
    /// </summary>
    /// <param name="suitOffset">Even byte offset 0, 2, or 4 into the Power, Varia, or Gravity suit table; not an unscaled suit ordinal.</param>
    /// <param name="phaseOffset">Even byte offset 0..10 into the six-word phase list.</param>
    public static bool IsCompiledChargePhase(int suitOffset, int phaseOffset) =>
        ValidSuit(suitOffset) && ValidChargePhase(phaseOffset);

    /// <summary>Whether $91:D829's reversed Hyper-shot glow list holds a frame at the byte offset, skipping its padding zero.</summary>
    /// <param name="tableOffset">Even byte offset 2..20 from the native list start, not a frame ordinal or palette-data byte offset.</param>
    public static bool IsCompiledHyperShot(int tableOffset) =>
        tableOffset >= FirstHyperTableByteOffset && tableOffset <= LastHyperTableByteOffset && (tableOffset & 1) == 0;

    /// <summary>Accepts only the three even byte offsets of the native suit pointer table.</summary>
    private static bool ValidSuit(int offset) => offset <= 4 && (offset & 1) == 0;

    /// <summary>Accepts only six word-aligned byte offsets in the charge phase list.</summary>
    private static bool ValidChargePhase(int offset) =>
        offset <= LastChargePhaseByteOffset && (offset & 1) == 0;
}
