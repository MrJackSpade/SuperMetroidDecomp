namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Immutable NTSC mechanics; the PAL cartridge has different movement records.</summary>
public static class BotwoonSpeedDefinitions
{
    /// <summary>
    /// Botwoon's speeds by native health phase: phase 0 is at least half health, 1 at least quarter, and 2 below quarter.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_B3.asm:
    /// movementSpeed=spitSpeed=p+2. The separate spacing proof shares this phase domain and the interleaved movement
    /// records. The second word of each movement record is segmentSpacingBytes=48/(p+2). The spacing division is exact
    /// for all three speeds and preserves the inverse relationship between speed and body travel time; these are not
    /// independent tuning arrays. Native health selection uses byte offset 4*p for movement and 2*p for spit.
    /// Separate speed and spacing proof tests compare all original words to these records.
    /// Do not extrapolate to another phase or apply this contract to PAL records.
    /// </remarks>
    public static readonly BotwoonSpeedDefinition[] ByHealthPhase =
    [
        new(2, 48 / 2, 2),
        new(3, 48 / 3, 3),
        new(4, 48 / 4, 4),
    ];
}
