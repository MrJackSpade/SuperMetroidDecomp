using static SuperMetroid.Core.Samus.Palettes.SamusChargePaletteDefinitions;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which charge steps and hyper shots the compiled charge palettes cover.</summary>
public static class SamusChargePalettes
{
    /// <summary>
    /// Whether $91:D7D5's charged-beam lists and $91:D7FF's pseudo-screw lists hold a palette for the step: each suit's
    /// list holds six.
    /// </summary>
    /// <param name="step">The charge cycle step.</param>
    public static bool IsCompiledChargeStep(int step) => (uint)step < ChargeSteps;

    /// <summary>Whether $91:D829's reversed Hyper-shot glow list holds a frame at the byte offset, skipping its padding zero.</summary>
    /// <param name="tableOffset">Even byte offset 2..20 from the native list start, not a frame ordinal or palette-data byte offset.</param>
    public static bool IsCompiledHyperShot(int tableOffset) =>
        tableOffset >= FirstHyperTableByteOffset && tableOffset <= LastHyperTableByteOffset && (tableOffset & 1) == 0;
}
