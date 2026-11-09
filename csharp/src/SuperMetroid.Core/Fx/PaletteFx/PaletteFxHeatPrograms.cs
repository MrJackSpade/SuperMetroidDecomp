using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The Norfair heat object's programs (#1298): a start per suit, and the sixteen phase programs per suit the heat
/// shimmer's condition restarts when the published heat palette index changes.
/// </summary>
public static class PaletteFxHeatPrograms
{
    /// <summary>The power suit's sixteen phase programs.</summary>
    internal static readonly PaletteFxProgram[] PowerPrograms =
    [
        PaletteFxProgram.SamusInHeatPowerSuitStep1,
        PaletteFxProgram.SamusInHeatPowerSuitStep2,
        PaletteFxProgram.SamusInHeatPowerSuitStep3,
        PaletteFxProgram.SamusInHeatPowerSuitStep4,
        PaletteFxProgram.SamusInHeatPowerSuitStep5,
        PaletteFxProgram.SamusInHeatPowerSuitStep6,
        PaletteFxProgram.SamusInHeatPowerSuitStep7,
        PaletteFxProgram.SamusInHeatPowerSuitStep8,
        PaletteFxProgram.SamusInHeatPowerSuitStep9,
        PaletteFxProgram.SamusInHeatPowerSuitStep10,
        PaletteFxProgram.SamusInHeatPowerSuitStep11,
        PaletteFxProgram.SamusInHeatPowerSuitStep12,
        PaletteFxProgram.SamusInHeatPowerSuitStep13,
        PaletteFxProgram.SamusInHeatPowerSuitStep14,
        PaletteFxProgram.SamusInHeatPowerSuitStep15,
        PaletteFxProgram.SamusInHeatPowerSuit10,
    ];

    /// <summary>The varia suit's sixteen phase programs.</summary>
    internal static readonly PaletteFxProgram[] VariaPrograms =
    [
        PaletteFxProgram.SamusInHeatVariaSuitStep2,
        PaletteFxProgram.SamusInHeatVariaSuitStep3,
        PaletteFxProgram.SamusInHeatVariaSuitStep4,
        PaletteFxProgram.SamusInHeatVariaSuitStep5,
        PaletteFxProgram.SamusInHeatVariaSuitStep6,
        PaletteFxProgram.SamusInHeatVariaSuitStep7,
        PaletteFxProgram.SamusInHeatVariaSuitStep8,
        PaletteFxProgram.SamusInHeatVariaSuitStep9,
        PaletteFxProgram.SamusInHeatVariaSuitStep10,
        PaletteFxProgram.SamusInHeatVariaSuitStep11,
        PaletteFxProgram.SamusInHeatVariaSuitStep12,
        PaletteFxProgram.SamusInHeatVariaSuitStep13,
        PaletteFxProgram.SamusInHeatVariaSuitStep14,
        PaletteFxProgram.SamusInHeatVariaSuitStep15,
        PaletteFxProgram.SamusInHeatVariaSuit10,
        PaletteFxProgram.SamusInHeatVariaSuit11,
    ];

    /// <summary>The gravity suit's sixteen phase programs.</summary>
    internal static readonly PaletteFxProgram[] GravityPrograms =
    [
        PaletteFxProgram.SamusInHeatGravitySuitStep2,
        PaletteFxProgram.SamusInHeatGravitySuitStep3,
        PaletteFxProgram.SamusInHeatGravitySuitStep4,
        PaletteFxProgram.SamusInHeatGravitySuitStep5,
        PaletteFxProgram.SamusInHeatGravitySuitStep6,
        PaletteFxProgram.SamusInHeatGravitySuitStep7,
        PaletteFxProgram.SamusInHeatGravitySuitStep8,
        PaletteFxProgram.SamusInHeatGravitySuitStep9,
        PaletteFxProgram.SamusInHeatGravitySuitStep10,
        PaletteFxProgram.SamusInHeatGravitySuitStep11,
        PaletteFxProgram.SamusInHeatGravitySuitStep12,
        PaletteFxProgram.SamusInHeatGravitySuitStep13,
        PaletteFxProgram.SamusInHeatGravitySuitStep14,
        PaletteFxProgram.SamusInHeatGravitySuitStep15,
        PaletteFxProgram.SamusInHeatGravitySuit10,
        PaletteFxProgram.SamusInHeatGravitySuit11,
    ];

    /// <summary>The heat object's start for <paramref name="suit"/>.</summary>
    public static PaletteFxProgram Start(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => PaletteFxProgram.SamusInHeatPowerSuitStep0,
        PaletteFxHeatSuit.Varia => PaletteFxProgram.SamusInHeatVariaSuitStep0,
        PaletteFxHeatSuit.Gravity => PaletteFxProgram.SamusInHeatGravitySuitStep0,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>The program of heat <paramref name="phase"/> for <paramref name="suit"/>.</summary>
    public static PaletteFxProgram Phase(PaletteFxHeatSuit suit, int phase)
    {
        PaletteFxProgram[] programs = suit switch
        {
            PaletteFxHeatSuit.Power => PowerPrograms,
            PaletteFxHeatSuit.Varia => VariaPrograms,
            PaletteFxHeatSuit.Gravity => GravityPrograms,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
        };
        if (phase < 0 || phase >= programs.Length)
            throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Norfair heat palette phase must be 0..{programs.Length - 1}.");
        return programs[phase];
    }

    /// <summary>The suit whose heat palette <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static PaletteFxHeatSuit SuitFor(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? PaletteFxHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? PaletteFxHeatSuit.Varia
        : PaletteFxHeatSuit.Power;
}
