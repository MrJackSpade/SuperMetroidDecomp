namespace SuperMetroid.Core.Game;

/// <summary>Immutable authored Bull movement parameters; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{

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
    /// <remarks>
    /// #625 / #649: the acceleration field in each four-byte record at
    /// $A8:D895-$D8C8 is exactly selector+3 for selector 0..12. All 13 first
    /// words match the pinned NTSC J/U v1.0 ROM and bank-A8 disassembly.
    /// The native initializer multiplies parameter one by four and copies this
    /// word to both the timer reset and live timer. Selector 13 would read the
    /// following code and is rejected.
    /// Deceleration at $A8:D897+4*selector advances every two selectors, with
    /// the two-frame interval held once more at selector four. Calculate the
    /// two half-rate segments on either side of that hold.
    /// VerifyCompiledBullMovement covers all 13 values through 104 real
    /// initializer combinations without a ROM bus.
    /// </remarks>
    public static (ushort Acceleration, ushort Deceleration) Intervals(ushort selector)
    {
        if (selector >= IntervalCount)
            throw new InvalidDataException($"Bull interval selector {selector} has no compiled authored entry.");
        int deceleration = selector < 4 ? (selector + 2) / 2 : (selector + 1) / 2;
        return ((ushort)(selector + 3), (ushort)deceleration);
    }
    /// <summary>
    /// $A8:D871 BullConstants_AngleToMove: convert the ten facing-dependent
    /// projectile directions to eight compass angles. Down occupies two slots;
    /// up appears at both ends. Each distinct direction advances a 32-unit octant.
    /// </summary>
    internal static ushort ShotAngle(int direction)
    {
        if ((uint)direction >= 10)
            throw new InvalidDataException($"Bull immune-shot reaction received invalid projectile direction {direction}.");
        int octant = direction - (direction >= 5 ? 1 : 0);
        return (ushort)((0xc0 + 0x20 * octant) & 0xff);
    }
}
