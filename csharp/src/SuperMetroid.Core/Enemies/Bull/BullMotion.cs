using static SuperMetroid.Core.Enemies.Bull.BullMovementDefinitions;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>A Bull's authored speed limit, speed-change intervals and shot-reaction angle.</summary>
public static class BullMotion
{
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
    public static int MaximumSpeed(int selector)
    {
        if (selector >= MaximumSpeedCount)
            throw new InvalidDataException($"Bull maximum-speed selector {selector} has no compiled authored entry.");
        return WordWrap.Unsigned(SlowestMaximumSpeed + selector * MaximumSpeedStep);
    }

    /// <summary>Returns the authored frame intervals selected by population parameter one.</summary>
    public static BullSpeedIntervals Intervals(int selector)
    {
        if (selector >= IntervalCount)
            throw new InvalidDataException($"Bull interval selector {selector} has no compiled authored entry.");
        return BullMovementDefinitions.Intervals[selector];
    }

    /// <summary>The compass angle an immune shot from projectile direction <paramref name="direction"/> moves the Bull at.</summary>
    internal static int ShotAngle(int direction)
    {
        if ((uint)direction >= ShotAngles.Length)
            throw new InvalidDataException($"Bull immune-shot reaction received invalid projectile direction {direction}.");
        return ShotAngles[direction];
    }
}
