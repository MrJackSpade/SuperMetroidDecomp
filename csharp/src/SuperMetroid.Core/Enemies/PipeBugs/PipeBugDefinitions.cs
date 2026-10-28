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
    /// $B3:882B/$8833: the normal/strong rising/shooting programs for the native facing and shooting flags. Every value
    /// names its actual behavior and species.
    /// </summary>
    internal static readonly Dictionary<PipeBugProgramKey, PipeBugProgram> BrinstarPrograms = new()
    {
        [new(false, PipeBugAnimationSelector.None)] = PipeBugProgram.ZebRisingLeft,
        [new(false, PipeBugAnimationSelector.Shooting)] = PipeBugProgram.ZebShootingLeft,
        [new(false, PipeBugAnimationSelector.FacingRight)] = PipeBugProgram.ZebRisingRight,
        [new(false, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting)] = PipeBugProgram.ZebShootingRight,
        [new(true, PipeBugAnimationSelector.None)] = PipeBugProgram.ZebboRisingLeft,
        [new(true, PipeBugAnimationSelector.Shooting)] = PipeBugProgram.ZebboShootingLeft,
        [new(true, PipeBugAnimationSelector.FacingRight)] = PipeBugProgram.ZebboRisingRight,
        [new(true, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting)] = PipeBugProgram.ZebboShootingRight,
    };
}
