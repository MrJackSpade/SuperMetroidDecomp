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

    /// <summary>
    /// The normal and strong rising and shooting segments for each Brinstar animation. Every value names its actual
    /// behavior and species.
    /// </summary>
    internal static readonly Dictionary<PipeBugAnimationKey, PipeBugSegment> BrinstarAnimations = new()
    {
        [new(false, PipeBugAnimationSelector.RisingFacingLeft)] = PipeBugSegment.ZebRisingLeft,
        [new(false, PipeBugAnimationSelector.ShootingFacingLeft)] = PipeBugSegment.ZebShootingLeft,
        [new(false, PipeBugAnimationSelector.RisingFacingRight)] = PipeBugSegment.ZebRisingRight,
        [new(false, PipeBugAnimationSelector.ShootingFacingRight)] = PipeBugSegment.ZebShootingRight,
        [new(true, PipeBugAnimationSelector.RisingFacingLeft)] = PipeBugSegment.ZebboRisingLeft,
        [new(true, PipeBugAnimationSelector.ShootingFacingLeft)] = PipeBugSegment.ZebboShootingLeft,
        [new(true, PipeBugAnimationSelector.RisingFacingRight)] = PipeBugSegment.ZebboRisingRight,
        [new(true, PipeBugAnimationSelector.ShootingFacingRight)] = PipeBugSegment.ZebboShootingRight,
    };
}
