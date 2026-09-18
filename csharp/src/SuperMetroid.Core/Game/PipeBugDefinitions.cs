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
    /// <summary>Normal Brinstar Pipe Bug enemy header at <c>$A0:F193</c>.</summary>
    internal const ushort BrinstarEnemyDefinition = 0xf193;

    /// <summary>Strong Brinstar Pipe Bug enemy header at <c>$A0:F1D3</c>.</summary>
    internal const ushort StrongBrinstarEnemyDefinition = 0xf1d3;

    /// <summary>Norfair Pipe Bug enemy header at <c>$A0:F213</c>.</summary>
    internal const ushort NorfairEnemyDefinition = 0xf213;

    /// <summary>Yellow Brinstar Pipe Bug enemy header at <c>$A0:F253</c>.</summary>
    internal const ushort YellowEnemyDefinition = 0xf253;

    /// <summary>
    /// Facing-left rising/shooting then facing-right rising/shooting instruction lists
    /// for the ordinary Zeb at <c>$B3:882B-$B3:8832</c>.
    /// </summary>
    private static readonly ushort[] BrinstarInstructionLists =
    [
        0x87ab,
        0x87cf,
        0x87eb,
        0x880f,
    ];

    /// <summary>
    /// Facing-left rising/shooting then facing-right rising/shooting instruction lists
    /// for the stronger Zebbo at <c>$B3:8833-$B3:883A</c>.
    /// </summary>
    private static readonly ushort[] StrongBrinstarInstructionLists =
    [
        0x8a1d,
        0x8a31,
        0x8a45,
        0x8a59,
    ];

    /// <summary>Returns one normal/strong Brinstar Pipe Bug animation program.</summary>
    internal static ushort BrinstarInstructionList(
        bool strong,
        PipeBugAnimationSelector selector)
    {
        int index = (int)selector;
        if ((uint)index >= BrinstarInstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Pipe Bug animation selector ${index:X4} exceeds its four-entry table.");
        }

        return (strong ? StrongBrinstarInstructionLists : BrinstarInstructionLists)[index];
    }
}
