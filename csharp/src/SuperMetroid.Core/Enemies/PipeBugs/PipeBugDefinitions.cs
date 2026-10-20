namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Fixed cartridge definitions shared by the three Pipe Bug families.</summary>
internal static class PipeBugDefinitions
{
    /// <summary>
    /// $B3:8C64-8C86 assigns NTSC formation release counters centered on104,
    /// eight ticks per vertical rank. Physical slot order is center, upper-near,
    /// upper-far, lower-near, lower-far; PAL shifts the same sequence down32 ticks.
    /// </summary>
    internal static ushort NorfairStaggerTarget(int member)
    {
        if ((uint)member >= 5) throw new ArgumentOutOfRangeException(nameof(member));
        int rank = member <= 2 ? -member : member - 2;
        return (ushort)(104 + 8 * rank);
    }

    /// <summary>
    /// $B3:8C87-8CA4 assigns each formation slot its center/upper/lower movement
    /// function. These are semantic native role branches, not ordinal addresses.
    /// </summary>
    internal static PipeBugEnemyFunction NorfairPostRiseFunction(int member) => member switch
    {
        0 => PipeBugEnemyFunction.NorfairLeaderStagger,
        1 => PipeBugEnemyFunction.NorfairUpperNearStagger,
        2 => PipeBugEnemyFunction.NorfairUpperFarStagger,
        3 => PipeBugEnemyFunction.NorfairLowerNearStagger,
        4 => PipeBugEnemyFunction.NorfairLowerFarStagger,
        _ => throw new ArgumentOutOfRangeException(nameof(member)),
    };

    /// <summary>
    /// $B3:882B/$8833 dispatch normal/strong rising/shooting programs by the native
    /// facing and shooting flags. Every value names its actual behavior and species.
    /// </summary>
    internal static PipeBugProgram BrinstarProgram(bool strong, PipeBugAnimationSelector selector) =>
        (strong, selector) switch
        {
            (false, PipeBugAnimationSelector.None) => PipeBugProgram.ZebRisingLeft,
            (false, PipeBugAnimationSelector.Shooting) => PipeBugProgram.ZebShootingLeft,
            (false, PipeBugAnimationSelector.FacingRight) => PipeBugProgram.ZebRisingRight,
            (false, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting) => PipeBugProgram.ZebShootingRight,
            (true, PipeBugAnimationSelector.None) => PipeBugProgram.ZebboRisingLeft,
            (true, PipeBugAnimationSelector.Shooting) => PipeBugProgram.ZebboShootingLeft,
            (true, PipeBugAnimationSelector.FacingRight) => PipeBugProgram.ZebboRisingRight,
            (true, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting) => PipeBugProgram.ZebboShootingRight,
            _ => throw new InvalidDataException(
                $"Pipe Bug animation selector ${(ushort)selector:X4} exceeds its four-entry table."),
        };
}
