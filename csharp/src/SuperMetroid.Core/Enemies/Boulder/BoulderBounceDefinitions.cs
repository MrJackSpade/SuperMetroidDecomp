namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Boulder quadratic-curve indices for successive ground impacts.</summary>
public static class BoulderBounceDefinitions
{
    /// <summary>
    /// The 8.8 bounce speed clock each impact restarts from, indexed by the remaining bounce count: the last impact
    /// installs the leading zero before the counter passes below zero into rolling.
    /// </summary>
    /// <remarks>
    /// #625 / #647 retained table: all three speeds are
    /// authored values. A formula for counters
    /// 1 and 2 needs a separate exception for counter 0, the last impact before rolling, so
    /// the three explicit choices state the collision sequence more clearly.
    /// The compiled boulder bounce check exercises each real floor collision, the
    /// zero-to-minus-one handoff, and both invalid counter boundaries.
    /// </remarks>
    internal static readonly int[] SpeedIndices = [0, 4096, 6144];
}
