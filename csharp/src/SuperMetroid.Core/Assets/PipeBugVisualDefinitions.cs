using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed bank-$B3 visual operand and its ordinary OAM frame identity.</summary>
internal readonly record struct PipeBugVisualSelector(ushort Address, ushort Frame);

/// <summary>
/// Compiled visual selections for Brinstar, Norfair, and yellow Pipe Bugs. Animation
/// durations, callbacks, launch behavior, and damage remain in the instruction programs.
/// </summary>
internal static class PipeBugVisualDefinitions
{
    /// <summary>Four normal Brinstar programs contribute twenty-eight visual operands.</summary>
    private const int NormalBrinstarSelectorCount = 28;

    private static readonly PipeBugVisualSelector[] Brinstar =
    [
        new(0x87ad, 0x89b7), new(0x87b1, 0x89be), new(0x87b5, 0x89c5),
        new(0x87b9, 0x89cc), new(0x87bd, 0x89d3), new(0x87c1, 0x89cc),
        new(0x87c5, 0x89c5), new(0x87c9, 0x89be), new(0x87d1, 0x89b7),
        new(0x87d5, 0x89be), new(0x87d9, 0x89cc), new(0x87dd, 0x89d3),
        new(0x87e1, 0x89cc), new(0x87e5, 0x89be), new(0x87ed, 0x89da),
        new(0x87f1, 0x89e1), new(0x87f5, 0x89e8), new(0x87f9, 0x89ef),
        new(0x87fd, 0x89f6), new(0x8801, 0x89ef), new(0x8805, 0x89e8),
        new(0x8809, 0x89e1), new(0x8811, 0x89da), new(0x8815, 0x89e1),
        new(0x8819, 0x89ef), new(0x881d, 0x89f6), new(0x8821, 0x89ef),
        new(0x8825, 0x89e1), new(0x8a1f, 0x8a82), new(0x8a23, 0x8a89),
        new(0x8a27, 0x8a90), new(0x8a2b, 0x8a89), new(0x8a33, 0x8a6d),
        new(0x8a37, 0x8a74), new(0x8a3b, 0x8a7b), new(0x8a3f, 0x8a74),
        new(0x8a47, 0x8aac), new(0x8a4b, 0x8ab3), new(0x8a4f, 0x8aba),
        new(0x8a53, 0x8ab3), new(0x8a5b, 0x8a97), new(0x8a5f, 0x8a9e),
        new(0x8a63, 0x8aa5), new(0x8a67, 0x8a9e),
    ];

    private static readonly PipeBugVisualSelector[] Norfair =
    [
        new(0x8ae3, 0x8e96), new(0x8ae7, 0x8e9d), new(0x8aeb, 0x8ea4),
        new(0x8aef, 0x8eab), new(0x8af3, 0x8eb2), new(0x8af7, 0x8eab),
        new(0x8afb, 0x8ea4), new(0x8aff, 0x8e9d), new(0x8b07, 0x8e96),
        new(0x8b0b, 0x8e9d), new(0x8b0f, 0x8eab), new(0x8b13, 0x8eb2),
        new(0x8b17, 0x8eab), new(0x8b1b, 0x8e9d), new(0x8b23, 0x8eb9),
        new(0x8b27, 0x8ec0), new(0x8b2b, 0x8ec7), new(0x8b2f, 0x8ece),
        new(0x8b33, 0x8ed5), new(0x8b37, 0x8ece), new(0x8b3b, 0x8ec7),
        new(0x8b3f, 0x8ec0), new(0x8b47, 0x8eb9), new(0x8b4b, 0x8ec0),
        new(0x8b4f, 0x8ece), new(0x8b53, 0x8ed5), new(0x8b57, 0x8ece),
        new(0x8b5b, 0x8ec0),
    ];

    private static readonly PipeBugVisualSelector[] Yellow =
    [
        new(0x8efe, 0x92ad), new(0x8f02, 0x92b4), new(0x8f06, 0x92bb),
        new(0x8f0a, 0x92b4), new(0x8f12, 0x92c2), new(0x8f16, 0x92c9),
        new(0x8f1a, 0x92d0), new(0x8f1e, 0x92c9), new(0x8f26, 0x92d7),
        new(0x8f2a, 0x92de), new(0x8f2e, 0x92e5), new(0x8f32, 0x92de),
        new(0x8f3a, 0x92ec), new(0x8f3e, 0x92f3), new(0x8f42, 0x92fa),
        new(0x8f46, 0x92f3),
    ];

    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        ReadOnlySpan<PipeBugVisualSelector> entries = enemyDefinition switch
        {
            PipeBugDefinitions.BrinstarEnemyDefinition =>
                Brinstar.AsSpan(..NormalBrinstarSelectorCount),
            PipeBugDefinitions.StrongBrinstarEnemyDefinition =>
                Brinstar.AsSpan(NormalBrinstarSelectorCount..),
            PipeBugDefinitions.NorfairEnemyDefinition => Norfair,
            PipeBugDefinitions.YellowEnemyDefinition => Yellow,
            _ => throw new InvalidDataException(
                $"Enemy ${enemyDefinition:X4} has no compiled Pipe Bug visuals."),
        };
        int low = 0;
        int high = entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PipeBugVisualSelector candidate = entries[middle];
            if (candidate.Address == address) return candidate.Frame;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Pipe Bug ${enemyDefinition:X4} visual operand $B3:{address:X4} is not compiled.");
    }
}
