namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Brinstar Pipe Bug animation selector stored in the native list-table index.
/// Bit zero selects shooting and bit one selects right-facing.
/// </summary>
[Flags]
public enum PipeBugAnimationSelector : ushort
{
    None = 0,
    Shooting = 1,
    FacingRight = 2,
}

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
        if ((uint)member >= 5) throw new IndexOutOfRangeException();
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
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>Normal Brinstar Pipe Bug enemy header at <c>$A0:F193</c>.</summary>
    internal const ushort BrinstarEnemyDefinition = 0xf193;

    /// <summary>Strong Brinstar Pipe Bug enemy header at <c>$A0:F1D3</c>.</summary>
    internal const ushort StrongBrinstarEnemyDefinition = 0xf1d3;

    /// <summary>Norfair Pipe Bug enemy header at <c>$A0:F213</c>.</summary>
    internal const ushort NorfairEnemyDefinition = 0xf213;

    /// <summary>Yellow Brinstar Pipe Bug enemy header at <c>$A0:F253</c>.</summary>
    internal const ushort YellowEnemyDefinition = 0xf253;

    /// <summary>
    /// $B3:882B/$8833 dispatch normal/strong rising/shooting programs by the native
    /// facing and shooting flags. Every value names its actual behavior and species.
    /// </summary>
    internal static ushort BrinstarInstructionList(bool strong, PipeBugAnimationSelector selector) =>
        (strong, selector) switch
        {
            (false, PipeBugAnimationSelector.None) => BrinstarPipeBugInstructionProgramDefinitions.NormalRisingLeft,
            (false, PipeBugAnimationSelector.Shooting) => BrinstarPipeBugInstructionProgramDefinitions.NormalShootingLeft,
            (false, PipeBugAnimationSelector.FacingRight) => BrinstarPipeBugInstructionProgramDefinitions.NormalRisingRight,
            (false, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting) => BrinstarPipeBugInstructionProgramDefinitions.NormalShootingRight,
            (true, PipeBugAnimationSelector.None) => BrinstarPipeBugInstructionProgramDefinitions.StrongRisingLeft,
            (true, PipeBugAnimationSelector.Shooting) => BrinstarPipeBugInstructionProgramDefinitions.StrongShootingLeft,
            (true, PipeBugAnimationSelector.FacingRight) => BrinstarPipeBugInstructionProgramDefinitions.StrongRisingRight,
            (true, PipeBugAnimationSelector.FacingRight | PipeBugAnimationSelector.Shooting) => BrinstarPipeBugInstructionProgramDefinitions.StrongShootingRight,
            _ => throw new InvalidDataException(
                $"Pipe Bug animation selector ${(ushort)selector:X4} exceeds its four-entry table."),
        };
}