namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed bank-$AA physical components for the shared right-facing Torizo
/// backward-jump frames at $AA:B048/B062/B07C. The visual OAM records are
/// independently replaceable, but the $AA:8A7A body rectangle is gameplay.
/// </summary>
internal static class TorizoJumpBackCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xb048, [new(15, -28, 0x87c7), new(4, -22, 0x89b8), new(0, 0, 0x8a7a)]),
        (0xb062, [new(15, -29, 0x87c7), new(4, -24, 0x89b6), new(0, 0, 0x8a7a)]),
        (0xb07c, [new(15, -29, 0x87c7), new(3, -24, 0x89b6), new(0, 0, 0x8a7a)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] Body =
        [new(-8, -37, 15, 14, 0xc977, 0xc97c)];

    internal static int FrameCount => Frames.Length;
    internal static ushort FramePointer(int index) => Frames[index].Pointer;

    internal static bool TryGetComponents(ushort frame,
        out ReadOnlyMemory<GoldenTorizoCollisionComponent> components)
    {
        foreach ((ushort pointer, GoldenTorizoCollisionComponent[] owned) in Frames)
        {
            if (pointer != frame) continue;
            components = owned;
            return true;
        }
        components = default;
        return false;
    }

    internal static bool HasFrame(ushort frame)
    {
        foreach ((ushort pointer, _) in Frames)
            if (pointer == frame) return true;
        return false;
    }

    internal static ReadOnlySpan<GoldenTorizoCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x8a7a => Body,
            0x87c7 or 0x89b6 or 0x89b8 => [],
            _ => throw new InvalidDataException(
                $"Torizo jump-back hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
