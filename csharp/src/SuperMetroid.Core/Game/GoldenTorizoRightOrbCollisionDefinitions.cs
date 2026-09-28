namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical components for the six extended Golden Torizo orb-attack frames
/// selected at $AA:CC99-CCDA. The body rectangle at $AA:8A88 and empty limb
/// rectangles remain engine-owned when their OAM art is replaced.
/// </summary>
internal static class GoldenTorizoRightOrbCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xac88, [new(15, -31, 0x87c7), new(4, -25, 0x89b6), new(0, 0, 0x8a88)]),
        (0xaca2, [new(9, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88)]),
        (0xacbc, [new(9, -31, 0x87c7), new(4, -25, 0x89d8), new(0, 0, 0x8a88)]),
        (0xacd6, [new(9, -31, 0x87c7), new(4, -25, 0x89e8), new(0, 0, 0x8a88)]),
        (0xacf0, [new(4, -25, 0x89f8), new(9, -31, 0x87c7), new(0, 0, 0x8a88)]),
        (0xad0a, [new(4, -25, 0x89f8), new(9, -31, 0x87c7), new(0, 0, 0x8a88)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] Body =
        [new(-9, -40, 16, 25, 0xc977, 0xc97c)];

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
            0x8a88 => Body,
            0x87c7 or 0x89b6 or 0x89c8 or 0x89d8 or 0x89e8 or 0x89f8 => [],
            _ => throw new InvalidDataException(
                $"Golden Torizo right orb hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
