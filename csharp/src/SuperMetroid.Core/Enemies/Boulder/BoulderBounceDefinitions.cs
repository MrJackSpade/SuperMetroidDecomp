namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Boulder quadratic-curve indices for successive ground impacts.</summary>
public static class BoulderBounceDefinitions
{
    /// <summary>
    /// 0, 4096, 6144, indexed by the remaining bounce count. The bounce
    /// routine indexes from table+2 with (remainingBounces-1)*2, so the last impact reads the first zero word before
    /// the counter underflows into rolling.
    /// </summary>
    /// <remarks>
    /// #625 / #647 retained table: all three words are
    /// authored values. A formula for counters
    /// 1 and 2 needs a separate exception for counter 0's rolling sentinel, so
    /// the three explicit choices state the collision sequence more clearly.
    /// VerifyCompiledBoulderBounces exercises each real floor collision, the
    /// zero-to-minus-one handoff, and both invalid counter boundaries.
    /// </remarks>
    internal static readonly ushort[] SpeedIndices = [0, 4096, 6144];
}
