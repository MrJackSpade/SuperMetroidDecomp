namespace SuperMetroid.Core.Game;

/// <summary>Authored angular velocities for Shaktool's seven physical segments.</summary>
public static class ShaktoolAngularVelocityDefinitions
{
    /// <summary>$AA:DEE9, initialCurlingNeighborAngleDelta: right saw through left saw.</summary>
    public const int ReferenceAddress = 0xaadee9;
    /// <summary>$AA:DEF7, zero: seven zero subtrahends used only by initialization.</summary>
    public const int InitialSubtractionReferenceAddress = 0xaadef7;

    /// <summary>Used unchanged at initialization and when synchronizing the group's orbit target.</summary>
    public static ushort ForSegment(int index) =>
        ShaktoolSegmentDefinitions.ForIndex(index).AngularVelocity;
}
