namespace SuperMetroid.Core.Game;

/// <summary>
/// Non-editable bank-$AA physical components and touch/shot rectangles for
/// Golden Torizo's ten walking-left frames at $AA:A4FA-A62C. These records
/// stay independent of the replaceable OAM compositions selected by the same
/// instruction lists.
/// </summary>
internal static class GoldenTorizoWalkingCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xa4fa, [new(-15, -30, 0x87c7), new(-5, -24, 0x8858), new(0, 0, 0x892a), new(-5, -24, 0x88dc)]),
        (0xa51c, [new(-15, -30, 0x87c7), new(-5, -25, 0x8858), new(0, 0, 0x884a), new(-5, -25, 0x88cc)]),
        (0xa53e, [new(-15, -31, 0x87c7), new(-5, -26, 0x885a), new(0, 0, 0x884a), new(-5, -26, 0x88bc)]),
        (0xa560, [new(-15, -32, 0x87c7), new(-5, -25, 0x886a), new(0, 0, 0x884a), new(-5, -25, 0x88ba)]),
        (0xa582, [new(-15, -32, 0x87c7), new(-5, -24, 0x887a), new(0, 0, 0x884a), new(-5, -24, 0x88ba)]),
        (0xa5a4, [new(-15, -30, 0x87c7), new(-5, -24, 0x887a), new(0, 0, 0x892a), new(-5, -24, 0x88ba)]),
        (0xa5c6, [new(-15, -31, 0x87c7), new(-5, -25, 0x886a), new(0, 0, 0x884a), new(-5, -25, 0x88ba)]),
        (0xa5e8, [new(-15, -31, 0x87c7), new(-5, -26, 0x885a), new(0, 0, 0x884a), new(-5, -26, 0x88bc)]),
        (0xa60a, [new(-15, -32, 0x87c7), new(-5, -25, 0x8858), new(0, 0, 0x884a), new(-5, -25, 0x88cc)]),
        (0xa62c, [new(-15, -31, 0x87c7), new(-5, -24, 0x8858), new(0, 0, 0x884a), new(-5, -24, 0x88dc)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] StandingBody =
        [new(-15, -39, 7, 21, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] SteppingBody =
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
            0x884a => StandingBody,
            0x892a => SteppingBody,
            0x87c7 or 0x8858 or 0x885a or 0x886a or 0x887a or
            0x88ba or 0x88bc or 0x88cc or 0x88dc => [],
            _ => throw new InvalidDataException(
                $"Golden Torizo walking hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
