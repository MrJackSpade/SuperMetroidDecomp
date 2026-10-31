namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Fixed cartridge definitions shared by the three Pipe Bug families.</summary>
internal static class PipeBugDefinitions
{
    /// <summary>
    /// $B3:8C64-8C86 assigns NTSC formation release counters centered on 104,
    /// eight ticks per vertical rank. Physical slot order is center, upper-near,
    /// upper-far, lower-near, lower-far; PAL shifts the same sequence down 32 ticks.
    /// </summary>
    internal static readonly ushort[] NorfairStaggerTargets = [104, 96, 88, 112, 120];

    /// <summary>
    /// $B3:8C87-8CA4 assigns each formation slot its center/upper/lower movement
    /// function. These are semantic native role branches, not ordinal addresses.
    /// </summary>
    internal static readonly PipeBugEnemyFunction[] NorfairPostRiseFunctions =
    [
        PipeBugEnemyFunction.NorfairLeaderStagger,
        PipeBugEnemyFunction.NorfairUpperNearStagger,
        PipeBugEnemyFunction.NorfairUpperFarStagger,
        PipeBugEnemyFunction.NorfairLowerNearStagger,
        PipeBugEnemyFunction.NorfairLowerFarStagger,
    ];

    /// <summary>
    /// The normal and strong rising and shooting programs for each Brinstar animation. Every value names its actual
    /// behavior and species.
    /// </summary>
    internal static readonly Dictionary<PipeBugProgramKey, PipeBugProgram> BrinstarPrograms = new()
    {
        [new(false, PipeBugAnimationSelector.RisingFacingLeft)] = PipeBugProgram.ZebRisingLeft,
        [new(false, PipeBugAnimationSelector.ShootingFacingLeft)] = PipeBugProgram.ZebShootingLeft,
        [new(false, PipeBugAnimationSelector.RisingFacingRight)] = PipeBugProgram.ZebRisingRight,
        [new(false, PipeBugAnimationSelector.ShootingFacingRight)] = PipeBugProgram.ZebShootingRight,
        [new(true, PipeBugAnimationSelector.RisingFacingLeft)] = PipeBugProgram.ZebboRisingLeft,
        [new(true, PipeBugAnimationSelector.ShootingFacingLeft)] = PipeBugProgram.ZebboShootingLeft,
        [new(true, PipeBugAnimationSelector.RisingFacingRight)] = PipeBugProgram.ZebboRisingRight,
        [new(true, PipeBugAnimationSelector.ShootingFacingRight)] = PipeBugProgram.ZebboShootingRight,
    };
}
