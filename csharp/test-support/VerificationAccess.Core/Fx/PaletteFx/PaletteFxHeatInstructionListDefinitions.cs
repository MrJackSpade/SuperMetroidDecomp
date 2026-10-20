namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable program selectors consumed by <c>PreInstruction_PaletteFXObject_SamusInHeat</c>.
/// </summary>
/// <remarks>
/// The Norfair palette animator publishes only phases zero through fifteen. These pointers
/// choose engine behavior and therefore belong to the compiled mechanics domain; the
/// selected programs' BGR555 color words remain cartridge-backed presentation data.
/// </remarks>
public static class PaletteFxHeatInstructionListDefinitions
{

    /// <summary>The sixteen phases published by Norfair palette program $8D:F08E.</summary>
    public const int PhaseCount = 16;

}
