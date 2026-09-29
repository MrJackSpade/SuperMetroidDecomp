namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical extended-frame components for both left-facing Golden Torizo
/// Chozo-orb attacks at $AA:CAFF-CB82. The OAM art may be replaced, but the
/// twelve component layouts and $AA:892A body hitbox remain engine-owned.
/// </summary>
internal static class GoldenTorizoLeftOrbCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xa64e, [new(-15, -31, 0x87c7), new(-4, -25, 0x885a), new(0, 0, 0x892a)]),
        (0xa668, [new(-9, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)]),
        (0xa682, [new(-9, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a)]),
        (0xa69c, [new(-9, -31, 0x87c7), new(-4, -25, 0x888a), new(0, 0, 0x892a)]),
        (0xa6b6, [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)]),
        (0xa6d0, [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)]),
        (0xa6ea, [new(-15, -31, 0x87c7), new(-4, -25, 0x8858), new(0, 0, 0x892a)]),
        (0xa704, [new(-9, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)]),
        (0xa71e, [new(-9, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a)]),
        (0xa738, [new(-9, -31, 0x87c7), new(-4, -25, 0x888a), new(0, 0, 0x892a)]),
        (0xa752, [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)]),
        (0xa76c, [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] Body =
        [new(-18, -37, 7, 18, 0xc977, 0xc97c)];

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
            0x892a => Body,
            0x87c7 or 0x8858 or 0x885a or 0x886a or 0x887a or 0x888a or 0x889a => [],
            _ => throw new InvalidDataException(
                $"Golden Torizo left-orb hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
