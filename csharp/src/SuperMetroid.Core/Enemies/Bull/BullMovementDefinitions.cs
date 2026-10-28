namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Immutable authored Bull movement parameters; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{

    /// <summary>Number of authored maximum-speed selectors.</summary>
    public const int MaximumSpeedCount = 8;

    /// <summary>Number of authored acceleration/deceleration selectors.</summary>
    public const int IntervalCount = 13;

    /// <summary>The 8.8 maximum speed selector zero chooses.</summary>
    internal const int SlowestMaximumSpeed = 0x03ff;

    /// <summary>How much faster each successive maximum-speed selector is, in 8.8.</summary>
    internal const int MaximumSpeedStep = 0x0100;

    /// <summary>
    /// The frame intervals each population parameter-one selector chooses.
    /// #625 / #649: the acceleration field in each four-byte record at
    /// $A8:D895-$D8C8 is exactly selector+3 for selector 0..12. All 13 first
    /// words match the pinned NTSC J/U v1.0 ROM and bank-A8 disassembly.
    /// The native initializer multiplies parameter one by four and copies this
    /// word to both the timer reset and live timer. Selector 13 would read the
    /// following code and is rejected.
    /// Deceleration at $A8:D897+4*selector advances every two selectors, with
    /// the two-frame interval held once more at selector four.
    /// </summary>
    internal static readonly BullSpeedIntervals[] Intervals =
    [
        new(3, 1), new(4, 1), new(5, 2), new(6, 2), new(7, 2), new(8, 3), new(9, 3),
        new(10, 4), new(11, 4), new(12, 5), new(13, 5), new(14, 6), new(15, 6),
    ];

    /// <summary>
    /// $A8:D871 BullConstants_AngleToMove: the compass angle each of the ten facing-dependent projectile directions
    /// moves the Bull at. Down occupies two slots; up appears at both ends; each distinct direction advances a 32-unit
    /// octant.
    /// </summary>
    internal static readonly ushort[] ShotAngles = [192, 224, 0, 32, 64, 64, 96, 128, 160, 192];
}
