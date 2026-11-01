using static SuperMetroid.Core.Samus.Palettes.SamusChargePaletteDefinitions;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which charge steps and hyper shots the compiled charge palettes cover.</summary>
public static class SamusChargePalettes
{
    /// <summary>
    /// Whether the charged-beam lists and the pseudo-screw lists hold a palette for the step: each suit's
    /// list holds six.
    /// </summary>
    /// <param name="step">The charge cycle step.</param>
    public static bool IsCompiledChargeStep(int step) => (uint)step < ChargeSteps;

    /// <summary>Whether the reversed Hyper-shot glow list holds a frame at the byte offset, skipping its padding zero.</summary>
    /// <param name="tableOffset">Even byte offset 2..20 from the list start, not a frame ordinal or palette-data byte offset.</param>
    public static bool IsCompiledHyperShot(int tableOffset) =>
        tableOffset >= FirstHyperTableByteOffset && tableOffset <= LastHyperTableByteOffset && (tableOffset & 1) == 0;
}
