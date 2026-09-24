using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed bank-$A2 Ripper-family visual operand and its OAM frame.</summary>
internal readonly record struct RipperVisualSelector(ushort Address, ushort Frame);

/// <summary>
/// Cartridge-authored frame selections for GRipper, Ripper II, and Ripper.
/// Direction reversal, motion, instruction timing, and freezing stay in game code.
/// </summary>
internal static class RipperVisualDefinitions
{
    private static readonly RipperVisualSelector[] Shared =
    [
        new(0xe19d, 0xe3c5), new(0xe1a1, 0xe3db),
        new(0xe1a5, 0xe3c5), new(0xe1a9, 0xe3ec),
        new(0xe1b1, 0xe402), new(0xe1b5, 0xe418),
        new(0xe1b9, 0xe402), new(0xe1bd, 0xe429),
        new(0xe2e2, 0xe3c5), new(0xe2e6, 0xe3db),
        new(0xe2ea, 0xe3c5), new(0xe2ee, 0xe3ec),
        new(0xe2f6, 0xe402), new(0xe2fa, 0xe418),
        new(0xe2fe, 0xe402), new(0xe302, 0xe429),
    ];

    private static readonly RipperVisualSelector[] Ordinary =
    [
        new(0xe479, 0xe54b), new(0xe47d, 0xe557),
        new(0xe481, 0xe54b), new(0xe485, 0xe563),
        new(0xe48d, 0xe527), new(0xe491, 0xe533),
        new(0xe495, 0xe527), new(0xe499, 0xe53f),
    ];

    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        ReadOnlySpan<RipperVisualSelector> entries = enemyDefinition switch
        {
            RoomEnemySystem.GRipperDefinition or RoomEnemySystem.Ripper2Definition => Shared,
            RoomEnemySystem.RipperDefinition => Ordinary,
            _ => throw new InvalidDataException(
                $"Enemy ${enemyDefinition:X4} has no compiled Ripper visuals."),
        };
        int low = 0;
        int high = entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RipperVisualSelector candidate = entries[middle];
            if (candidate.Address == address) return candidate.Frame;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Ripper-family ${enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
