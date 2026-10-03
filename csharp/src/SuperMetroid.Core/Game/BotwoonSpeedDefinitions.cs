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
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_B3.asm:
    /// validate p=0..2; movementSpeed=spitSpeed=p+2. The separate spacing proof
    /// shares this phase domain and the interleaved movement records.
    /// The second word of each movement record is segmentSpacingBytes=48/(p+2). The spacing
    /// division is exact for all three speeds and preserves the inverse relationship
    /// between speed and body travel time; these are not independent tuning arrays.
    /// Native health selection uses byte offset 4*p for movement and 2*p for spit.
    /// Separate speed and spacing proof tests compare all original words to this API.
    /// Do not extrapolate to another phase or apply this contract to PAL records.
    /// Integer arithmetic is exact; there is no rounding or floating-point approximation.
    /// </remarks>
    public static BotwoonSpeedDefinition ForHealthPhase(byte phase)
    {
        if (phase > 2)
            throw new InvalidDataException($"Botwoon health phase {phase} is outside the three native speed records.");
        ushort speed = (ushort)(phase + 2);
        return new(speed, (ushort)(48 / speed), speed);
    }
}
