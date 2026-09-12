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
    public static BotwoonSpeedDefinition ForHealthPhase(byte phase) => phase switch
    {
        0 => new(2, 0x18, 2),
        1 => new(3, 0x10, 3),
        2 => new(4, 0x0c, 4),
        _ => throw new InvalidDataException($"Botwoon health phase {phase} is outside the three native speed records."),
    };
}
