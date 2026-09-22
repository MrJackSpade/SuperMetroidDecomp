namespace SuperMetroid.Core.Game;

/// <summary>Immutable authored Bull movement parameters; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{
    /// <summary>$A8:D885, BullConstants_MaxSpeeds, eight 8.8 maximum-speed words.</summary>
    public const int MaximumSpeedReferenceAddress = 0xa8d885;

    /// <summary>$A8:D895, BullAccelerationIntervalTable, thirteen acceleration/deceleration pairs.</summary>
    public const int IntervalReferenceAddress = 0xa8d895;

    /// <summary>Number of authored maximum-speed selectors.</summary>
    public const int MaximumSpeedCount = 8;

    /// <summary>Number of authored acceleration/deceleration selectors.</summary>
    public const int IntervalCount = 13;

    /// <summary>Returns the authored 8.8 speed selected by population parameter two.</summary>
    /// <remarks>
    /// #625 / #648 exact NTSC J/U v1.0 algorithm: for selector i in 0..7,
    /// the unsigned word at $A8:D885 + 2*i is $03FF + i*$0100. All eight
    /// words match the pinned ROM and bank-A8 disassembly. The native initializer
    /// doubles parameter two for a word offset; selector 8 would read the
    /// adjacent acceleration table and is outside the speed domain.
    /// VerifyCompiledBullMovement covers all eight values through 104 real
    /// initializer combinations and rejects selector 8 without a ROM bus.
    /// </remarks>
    public static ushort MaximumSpeed(ushort selector)
    {
        if (selector >= MaximumSpeedCount)
            throw new InvalidDataException($"Bull maximum-speed selector {selector} has no compiled authored entry.");
        return (ushort)(0x03ff + selector * 0x0100);
    }

    /// <summary>Returns the authored frame intervals selected by population parameter one.</summary>
    public static (ushort Acceleration, ushort Deceleration) Intervals(ushort selector)
    {
        if (selector >= IntervalCount)
            throw new InvalidDataException($"Bull interval selector {selector} has no compiled authored entry.");
        ReadOnlySpan<ushort> deceleration = [1, 1, 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6];
        return ((ushort)(selector + 3), deceleration[selector]);
    }
}
