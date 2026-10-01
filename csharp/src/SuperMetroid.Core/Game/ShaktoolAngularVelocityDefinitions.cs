namespace SuperMetroid.Core.Game;

/// <summary>Authored angular velocities for Shaktool's seven physical segments.</summary>
public static class ShaktoolAngularVelocityDefinitions
{
    /// <summary>$AA:DEE9, initialCurlingNeighborAngleDelta: right saw through left saw.</summary>
    public const int ReferenceAddress = 0xaadee9;
    /// <summary>$AA:DEF7, zero: seven zero subtrahends used only by initialization.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all seven NTSC J/U v1.0 words and
    /// pinned bank_AA.asm's SBC at $AA:DE76. Subtracting these zeros is an identity;
    /// compiled initialization already uses the selected angular velocity directly.
    /// This is an existing conversion, not a new table elimination in this review.
    /// </remarks>
    public const int InitialSubtractionReferenceAddress = 0xaadef7;

    /// <summary>Used unchanged at initialization and when synchronizing the group's orbit target.</summary>
    public static ushort ForSegment(int index) =>
        ShaktoolSegmentDefinitions.ForIndex(index).AngularVelocity;
}
