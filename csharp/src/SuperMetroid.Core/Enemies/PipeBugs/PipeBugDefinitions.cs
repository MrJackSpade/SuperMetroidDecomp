namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Fixed authored definitions shared by the three Pipe Bug families.</summary>
internal static class PipeBugDefinitions
{
    /// <summary>
    /// The formation initializer assigns formation release counters centered on 104,
    /// eight ticks per vertical rank. Listing order is center, upper-near,
    /// upper-far, lower-near, lower-far; the European release shifts the same sequence down 32 ticks.
    /// </summary>
    internal static readonly int[] NorfairStaggerTargets = [104, 96, 88, 112, 120];

    /// <summary>
    /// The formation initializer assigns each formation slot its center/upper/lower movement
    /// action. These are semantic role branches, not ordinal positions.
    /// </summary>
    internal static readonly PipeBugAction[] NorfairPostRiseActions =
    [
        PipeBugAction.NorfairLeaderStagger,
        PipeBugAction.NorfairUpperNearStagger,
        PipeBugAction.NorfairUpperFarStagger,
        PipeBugAction.NorfairLowerNearStagger,
        PipeBugAction.NorfairLowerFarStagger,
    ];
}
