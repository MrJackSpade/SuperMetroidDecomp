namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled row-selection schedule for Ceres Ridley's eye fade. The selected palette
/// colors remain presentation data; this catalog owns only the fixed row and completion
/// metadata consumed by <c>CeresRidley_Func_3</c> at <c>$A6:A389</c>.
/// </summary>
internal static class CeresRidleyEyeFadeDefinitions
{
    /// <summary>
    /// <c>RidleyFadeIn_EyeGlowIntensity</c> at $A6:E269: 64 palette-row selectors
    /// followed by the $FF completion marker at $A6:E2A9.
    /// </summary>
    public const int NativeScheduleAddress = 0xa6e269;

    /// <summary>Number of palette-producing entries before the native terminator.</summary>
    public const ushort PaletteStepCount = 64;

    /// <summary>
    /// Resolves one authored scheduler offset. The first sixteen frames select rows
    /// fifteen down through zero; the remaining 48 frames hold row zero before completion.
    /// </summary>
    public static CeresRidleyEyeFadeStep Get(ushort offset) => offset switch
    {
        < 16 => new(false, unchecked((byte)(15 - offset))),
        < PaletteStepCount => new(false, 0),
        PaletteStepCount => new(true, 0),
        _ => throw new InvalidDataException(
            $"Ceres Ridley eye-fade offset {offset} exceeds the authored " +
            $"{PaletteStepCount + 1}-entry schedule."),
    };
}

/// <summary>One Ceres Ridley eye-fade row selection or the terminal handoff.</summary>
internal readonly record struct CeresRidleyEyeFadeStep(bool IsComplete, byte PaletteRow);
