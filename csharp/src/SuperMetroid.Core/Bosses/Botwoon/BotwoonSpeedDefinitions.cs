namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Immutable NTSC mechanics; the PAL cartridge has different movement records.</summary>
public static class BotwoonSpeedDefinitions
{
    /// <summary>
    /// Botwoon's speeds by health phase: phase 0 is at least half health, 1 at least quarter, and 2 below quarter.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_B3.asm: movement and spit speed are
    /// p+2, and the body spacing is 12/(p+2) history entries. The spacing division is exact for all three speeds and
    /// preserves the inverse relationship between speed and body travel time; these are not independent tuning arrays.
    /// Do not extrapolate to another phase or apply this contract to PAL records.
    /// </remarks>
    public static readonly BotwoonSpeedDefinition[] ByHealthPhase =
    [
        new(2, 12 / 2, 2),
        new(3, 12 / 3, 3),
        new(4, 12 / 4, 4),
    ];
}
