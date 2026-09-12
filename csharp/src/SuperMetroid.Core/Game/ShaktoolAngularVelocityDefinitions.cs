namespace SuperMetroid.Core.Game;

/// <summary>Authored angular velocities for Shaktool's seven physical segments.</summary>
public static class ShaktoolAngularVelocityDefinitions
{
    /// <summary>$AA:DEE9, initialCurlingNeighborAngleDelta: right saw through left saw.</summary>
    public const int ReferenceAddress = 0xaadee9;
    /// <summary>$AA:DEF7, zero: seven zero subtrahends used only by initialization.</summary>
    public const int InitialSubtractionReferenceAddress = 0xaadef7;

    /// <summary>Used unchanged at initialization and when synchronizing the group's orbit target.</summary>
    public static ushort ForSegment(int index) => index switch
    {
        0 => 0,
        1 => 0x20,
        2 => 0x60,
        3 => 0xc0,
        4 => 0x140,
        5 => 0x1a0,
        6 => 0x1e0,
        _ => throw new InvalidDataException($"Shaktool segment {index} is outside the seven native angular records."),
    };
}
