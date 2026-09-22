namespace SuperMetroid.Core.Game;

/// <summary>Native Boulder quadratic-curve indices for successive ground impacts.</summary>
public static class BoulderBounceDefinitions
{
    /// <summary>
    /// $A6:86EF, BounceSpeedTableIndices: $0000, $1000, $1800. At $A6:88EA the
    /// routine indexes from table+2 with (remainingBounces-1)*2, so the last
    /// impact reads the first zero word before the counter underflows into rolling.
    /// </summary>
    /// <remarks>
    /// #625 / #647 retained table: all three words at $A6:86EF-$86F4 match the
    /// pinned NTSC J/U v1.0 ROM and bank-A6 disassembly. A formula for counters
    /// 1 and 2 needs a separate exception for counter 0's rolling sentinel, so
    /// the three explicit choices state the collision sequence more clearly.
    /// VerifyCompiledBoulderBounces exercises each real floor collision, the
    /// zero-to-$FFFF handoff, and both invalid counter boundaries without a bus.
    /// </remarks>
    public static ushort SpeedIndex(ushort remainingBounces) => remainingBounces switch
    {
        2 => 0x1800,
        1 => 0x1000,
        0 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(remainingBounces)),
    };
}
