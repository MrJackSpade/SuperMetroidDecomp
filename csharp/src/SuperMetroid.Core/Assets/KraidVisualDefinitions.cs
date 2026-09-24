using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed Kraid-family visual operand and its ordinary OAM frame identity.</summary>
internal readonly record struct KraidVisualSelector(ushort Address, ushort Frame);

/// <summary>
/// Compiled Fake Kraid and fingernail frame selections. These select installed art;
/// collision, nail flight, action callbacks, and instruction timing stay in gameplay code.
/// </summary>
internal static class KraidVisualDefinitions
{
    /// <summary>Initial fingernail frame named by `$A7:8B0C` at `$A7:A617`.</summary>
    internal const ushort InitialNailFrame = 0xa617;

    private static readonly KraidVisualSelector[] Nail =
    [
        new(0x8b0c, 0xa617), new(0x8b10, 0xa623),
        new(0x8b14, 0xa639), new(0x8b18, 0xa645),
        new(0x8b1c, 0xa65b), new(0x8b20, 0xa667),
        new(0x8b24, 0xa67d), new(0x8b28, 0xa689),
    ];

    private static readonly KraidVisualSelector[] FakeKraid =
    [
        new(0x99b0, 0x9c64), new(0x99b4, 0x9cb6),
        new(0x99b8, 0x9d08), new(0x99bc, 0x9d5a),
        new(0x99c8, 0x9c64), new(0x99ce, 0x9d5a),
        new(0x99d2, 0x9d08), new(0x99d6, 0x9cb6),
        new(0x99de, 0x9dac), new(0x99e4, 0x9dfe),
        new(0x99ea, 0x9e50), new(0x99ee, 0x9dfe),
        new(0x99fe, 0x9ea2), new(0x9a02, 0x9ef4),
        new(0x9a06, 0x9f46), new(0x9a0a, 0x9f98),
        new(0x9a16, 0x9ea2), new(0x9a1c, 0x9f98),
        new(0x9a20, 0x9f46), new(0x9a24, 0x9ef4),
        new(0x9a2c, 0x9fea), new(0x9a32, 0xa03c),
        new(0x9a38, 0xa08e), new(0x9a3c, 0xa03c),
    ];

    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        ReadOnlySpan<KraidVisualSelector> entries = enemyDefinition switch
        {
            RoomEnemySystem.FakeKraidDefinition => FakeKraid,
            RoomEnemySystem.KraidGoodNailDefinition or
                RoomEnemySystem.KraidBadNailDefinition => Nail,
            _ => throw new InvalidDataException(
                $"Enemy ${enemyDefinition:X4} has no compiled Kraid-family visuals."),
        };
        int low = 0;
        int high = entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KraidVisualSelector candidate = entries[middle];
            if (candidate.Address == address) return candidate.Frame;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Kraid-family ${enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
