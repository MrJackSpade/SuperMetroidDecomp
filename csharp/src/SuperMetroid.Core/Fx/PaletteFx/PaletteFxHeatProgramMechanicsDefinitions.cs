namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words for the three Samus-in-heat palette programs in bank $8D.
/// </summary>
/// <remarks>
/// Each timed record contains fifteen BGR555 colors between its duration and terminal
/// wait command. Those 720 color words remain live presentation data; this catalog owns
/// only program setup, timing, waits, and loop control.
/// Power, Varia, and Gravity enter at $8D:E45E, $E68A, and $E8B6;
/// each installs heat pre-instruction $E379 and CGRAM byte $0182.
/// Their sixteen records start at $E466, $E692, or $E8BE plus $22*p,
/// p=0..15, and each ends in $C595 wait. Gotos at $E686, $E8B2,
/// and $EADE return to phase zero; p=16 reaches control. All 114
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class PaletteFxHeatProgramMechanicsDefinitions
{
    /// <summary>Compiled heat palette control for the Power Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Power = new()
    {
        Suit = PaletteFxHeatSuit.Power,
        Rows = PaletteFxColorRows.Of(PaletteFxProgram.SamusInHeatPowerSuit1, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>Compiled heat palette control for the Varia Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Varia = new()
    {
        Suit = PaletteFxHeatSuit.Varia,
        Rows = PaletteFxColorRows.Of(PaletteFxProgram.SamusInHeatVariaSuit2, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };
    /// <summary>Compiled heat palette control for the Gravity Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Gravity = new()
    {
        Suit = PaletteFxHeatSuit.Gravity,
        Rows = PaletteFxColorRows.Of(PaletteFxProgram.SamusInHeatGravitySuit2, PaletteFxHeatProgramDefinition.ColorsPerFrame),
    };

    /// <summary>The Power, Varia, and Gravity programs in native selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All { get; } = [Power, Varia, Gravity];

}
