using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The Norfair heat object's programs (#1298): a start per suit, and the sixteen phase programs per suit the heat
/// pre-instruction restarts when the published heat palette index changes.
/// </summary>
public static class PaletteFxHeatPrograms
{
    /// <summary>The power suit's sixteen phase programs.</summary>
    private static readonly PaletteFxProgram[] PowerPrograms =
    [
        PaletteFxProgram.SamusInHeatPowerSuit1,
        PaletteFxProgram.SamusInHeatPowerSuit2,
        PaletteFxProgram.SamusInHeatPowerSuit3,
        PaletteFxProgram.SamusInHeatPowerSuit4,
        PaletteFxProgram.SamusInHeatPowerSuit5,
        PaletteFxProgram.SamusInHeatPowerSuit6,
        PaletteFxProgram.SamusInHeatPowerSuit7,
        PaletteFxProgram.SamusInHeatPowerSuit8,
        PaletteFxProgram.SamusInHeatPowerSuit9,
        PaletteFxProgram.SamusInHeatPowerSuitA,
        PaletteFxProgram.SamusInHeatPowerSuitB,
        PaletteFxProgram.SamusInHeatPowerSuitC,
        PaletteFxProgram.SamusInHeatPowerSuitD,
        PaletteFxProgram.SamusInHeatPowerSuitE,
        PaletteFxProgram.SamusInHeatPowerSuitF,
        PaletteFxProgram.SamusInHeatPowerSuit10,
    ];

    /// <summary>The varia suit's sixteen phase programs.</summary>
    private static readonly PaletteFxProgram[] VariaPrograms =
    [
        PaletteFxProgram.SamusInHeatVariaSuit2,
        PaletteFxProgram.SamusInHeatVariaSuit3,
        PaletteFxProgram.SamusInHeatVariaSuit4,
        PaletteFxProgram.SamusInHeatVariaSuit5,
        PaletteFxProgram.SamusInHeatVariaSuit6,
        PaletteFxProgram.SamusInHeatVariaSuit7,
        PaletteFxProgram.SamusInHeatVariaSuit8,
        PaletteFxProgram.SamusInHeatVariaSuit9,
        PaletteFxProgram.SamusInHeatVariaSuitA,
        PaletteFxProgram.SamusInHeatVariaSuitB,
        PaletteFxProgram.SamusInHeatVariaSuitC,
        PaletteFxProgram.SamusInHeatVariaSuitD,
        PaletteFxProgram.SamusInHeatVariaSuitE,
        PaletteFxProgram.SamusInHeatVariaSuitF,
        PaletteFxProgram.SamusInHeatVariaSuit10,
        PaletteFxProgram.SamusInHeatVariaSuit11,
    ];

    /// <summary>The gravity suit's sixteen phase programs.</summary>
    private static readonly PaletteFxProgram[] GravityPrograms =
    [
        PaletteFxProgram.SamusInHeatGravitySuit2,
        PaletteFxProgram.SamusInHeatGravitySuit3,
        PaletteFxProgram.SamusInHeatGravitySuit4,
        PaletteFxProgram.SamusInHeatGravitySuit5,
        PaletteFxProgram.SamusInHeatGravitySuit6,
        PaletteFxProgram.SamusInHeatGravitySuit7,
        PaletteFxProgram.SamusInHeatGravitySuit8,
        PaletteFxProgram.SamusInHeatGravitySuit9,
        PaletteFxProgram.SamusInHeatGravitySuitA,
        PaletteFxProgram.SamusInHeatGravitySuitB,
        PaletteFxProgram.SamusInHeatGravitySuitC,
        PaletteFxProgram.SamusInHeatGravitySuitD,
        PaletteFxProgram.SamusInHeatGravitySuitE,
        PaletteFxProgram.SamusInHeatGravitySuitF,
        PaletteFxProgram.SamusInHeatGravitySuit10,
        PaletteFxProgram.SamusInHeatGravitySuit11,
    ];

    /// <summary><c>$8D:E440</c>: the heat object's start for <paramref name="suit"/>.</summary>
    public static PaletteFxProgram Start(PaletteFxHeatSuit suit) => suit switch
    {
        PaletteFxHeatSuit.Power => PaletteFxProgram.SamusInHeatPowerSuit0,
        PaletteFxHeatSuit.Varia => PaletteFxProgram.SamusInHeatVariaSuit0,
        PaletteFxHeatSuit.Gravity => PaletteFxProgram.SamusInHeatGravitySuit0,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
    };

    /// <summary>The program of heat <paramref name="phase"/> for <paramref name="suit"/>.</summary>
    public static PaletteFxProgram Phase(PaletteFxHeatSuit suit, ushort phase)
    {
        PaletteFxProgram[] programs = suit switch
        {
            PaletteFxHeatSuit.Power => PowerPrograms,
            PaletteFxHeatSuit.Varia => VariaPrograms,
            PaletteFxHeatSuit.Gravity => GravityPrograms,
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, "Unknown heat suit."),
        };
        if (phase >= programs.Length)
            throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Norfair heat palette phase must be 0..{programs.Length - 1}.");
        return programs[phase];
    }

    /// <summary>The suit whose heat palette <paramref name="equippedItems"/> selects: Gravity before Varia.</summary>
    public static PaletteFxHeatSuit SuitFor(ushort equippedItems) =>
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit) ? PaletteFxHeatSuit.Gravity
        : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? PaletteFxHeatSuit.Varia
        : PaletteFxHeatSuit.Power;
}
