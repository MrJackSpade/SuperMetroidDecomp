namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's immutable movement speeds; the European release moves him at other speeds.</summary>
public static class BotwoonSpeedDefinitions
{
    /// <summary>
    /// Botwoon's speeds by health phase: phase 0 is at least half health, 1 at least quarter, and 2 below quarter.
    /// </summary>
    /// <remarks>
    /// Movement and spit speed are the phase plus two, and the body spacing is twelve divided by that speed, in history
    /// entries. The spacing division is exact for all three speeds and preserves the inverse relationship between speed
    /// and body travel time; these are not independent tuning arrays.
    /// Do not extrapolate to another phase or apply this contract to the European release.
    /// </remarks>
    public static readonly BotwoonSpeedDefinition[] ByHealthPhase =
    [
        new(2, 12 / 2, 2),
        new(3, 12 / 3, 3),
        new(4, 12 / 4, 4),
    ];
}
