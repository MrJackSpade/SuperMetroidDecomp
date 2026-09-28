namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned physical components and touch/shot rectangles for Golden
/// Torizo's turning-right and walking-right frames at $AA:A4F0 and
/// $AA:AA98-ABCA. Their editable OAM counterparts do not own collision.
/// </summary>
internal static class GoldenTorizoRightwardCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xa4f0, [new(0, 0, 0x87c7)]),
        (0xaa98, [new(15, -30, 0x87c7), new(5, -24, 0x89b6), new(0, 0, 0x8a88), new(5, -24, 0x8a3a)]),
        (0xaaba, [new(15, -30, 0x87c7), new(5, -25, 0x89b6), new(0, 0, 0x89a8), new(5, -25, 0x8a2a)]),
        (0xaadc, [new(15, -31, 0x87c7), new(5, -26, 0x89b8), new(0, 0, 0x89a8), new(5, -26, 0x8a1a)]),
        (0xaafe, [new(15, -32, 0x87c7), new(5, -25, 0x89c8), new(0, 0, 0x89a8), new(5, -25, 0x8a18)]),
        (0xab20, [new(15, -32, 0x87c7), new(5, -24, 0x89d8), new(0, 0, 0x89a8), new(5, -24, 0x8a18)]),
        (0xab42, [new(15, -30, 0x87c7), new(5, -24, 0x89d8), new(0, 0, 0x8a88), new(5, -24, 0x8a18)]),
        (0xab64, [new(15, -31, 0x87c7), new(5, -25, 0x89c8), new(0, 0, 0x89a8), new(5, -25, 0x8a18)]),
        (0xab86, [new(15, -31, 0x87c7), new(5, -26, 0x89b8), new(0, 0, 0x89a8), new(5, -26, 0x8a1a)]),
        (0xaba8, [new(15, -32, 0x87c7), new(5, -25, 0x89b6), new(0, 0, 0x89a8), new(5, -25, 0x8a2a)]),
        (0xabca, [new(15, -31, 0x87c7), new(5, -24, 0x89b6), new(0, 0, 0x89a8), new(5, -24, 0x8a3a)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] StandingBody =
        [new(-10, -38, 13, 23, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] SteppingBody =
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
            0x89a8 => StandingBody,
            0x8a88 => SteppingBody,
            0x87c7 or 0x89b6 or 0x89b8 or 0x89c8 or 0x89d8 or
            0x8a18 or 0x8a1a or 0x8a2a or 0x8a3a => [],
            _ => throw new InvalidDataException(
                $"Golden Torizo rightward hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
