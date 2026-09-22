namespace SuperMetroid.Core.Game;

/// <summary>Botwoon's health-dependent movement, body-history spacing and spit speed.</summary>
public readonly record struct BotwoonSpeedDefinition(ushort MovementSpeed, ushort SegmentSpacingBytes, ushort SpitSpeed);

/// <summary>Immutable NTSC mechanics; the PAL cartridge has different movement records.</summary>
public static class BotwoonSpeedDefinitions
{
    /// <summary>$B3:94BB, BotwoonSpeedTable: three speed/body-travel-time pairs.</summary>
    public const int MovementReferenceAddress = 0xb394bb;
    /// <summary>$B3:9E77, BotwoonSpitSpeeds: three health-phase projectile speeds.</summary>
    public const int SpitReferenceAddress = 0xb39e77;

    /// <summary>Native phase 0 is at least half health, 1 at least quarter, and 2 below quarter.</summary>
    /// <remarks>
    /// Issue #625 exact NTSC algorithms for the two native tables: validate p=0..2;
    /// movementSpeed=spitSpeed=p+2, segmentSpacingBytes=48/(p+2). The spacing
    /// division is exact for all three speeds and preserves the inverse relationship
    /// between speed and body travel time; these are not independent tuning arrays.
    /// LookupTableResearch verifies all six $B3:94BB words and all three $B3:9E77
    /// words against the NTSC J/U v1.0 ROM, pinned bank_B3.asm and this selector.
    /// Do not extrapolate to another phase or apply this contract to PAL records.
    /// Runtime replacement is deferred; no floating-point approximation is needed.
    /// </remarks>
    public static BotwoonSpeedDefinition ForHealthPhase(byte phase) => phase switch
    {
        0 => new(2, 0x18, 2),
        1 => new(3, 0x10, 3),
        2 => new(4, 0x0c, 4),
        _ => throw new InvalidDataException($"Botwoon health phase {phase} is outside the three native speed records."),
    };
}
