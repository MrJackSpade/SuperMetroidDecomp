using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One fixed bank-$A2 visual operand and its ordinary OAM frame.</summary>
internal readonly record struct OwtchStokeVisualSelector(ushort Address, ushort Frame);

/// <summary>
/// Owtch and Stoke frame selection from their native instruction lists. The
/// instruction callbacks, timing, and projectile behavior remain in game code.
/// </summary>
internal static class OwtchStokeVisualDefinitions
{
    private static readonly OwtchStokeVisualSelector[] Owtch =
    [
        new(0xa3af, 0xa589), new(0xa3b3, 0xa590), new(0xa3b7, 0xa597),
        new(0xa3c1, 0xa597), new(0xa3c5, 0xa590), new(0xa3c9, 0xa589),
    ];

    private static readonly OwtchStokeVisualSelector[] Stoke =
    [
        new(0x8936, 0x8aca), new(0x893a, 0x8ad6),
        new(0x893e, 0x8ae7), new(0x8942, 0x8af3),
        new(0x894a, 0x8ae7), new(0x8952, 0x8aff),
        new(0x895c, 0x8b15), new(0x8960, 0x8b21),
        new(0x8964, 0x8b32), new(0x8968, 0x8b3e),
        new(0x8970, 0x8b32), new(0x8978, 0x8b4a),
    ];

    internal static ushort FrameAt(ushort enemyDefinition, ushort address)
    {
        ReadOnlySpan<OwtchStokeVisualSelector> entries = enemyDefinition switch
        {
            RoomEnemySystem.OwtchDefinition => Owtch,
            RoomEnemySystem.StokeDefinition => Stoke,
            _ => throw new InvalidDataException(
                $"Enemy ${enemyDefinition:X4} has no compiled Owtch/Stoke visuals."),
        };
        int low = 0;
        int high = entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            OwtchStokeVisualSelector candidate = entries[middle];
            if (candidate.Address == address) return candidate.Frame;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException(
            $"Owtch/Stoke ${enemyDefinition:X4} visual operand ${address:X4} is not compiled.");
    }
}
