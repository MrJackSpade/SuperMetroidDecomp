namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned extended-frame offsets and hitbox lists for the two
/// right-facing Golden Torizo sonic-boom programs at $AA:CCDB-CDAE.
/// OAM spritemap components are separate editable presentation data.
/// </summary>
internal static class GoldenTorizoRightSonicCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xabec, [new(15, -31, 0x87c7), new(4, -25, 0x89b8), new(0, 0, 0x8a88)]),
        (0xad24, [new(15, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88)]),
        (0xad3e, [new(15, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88), new(4, -25, 0x8a3a)]),
        (0xad60, [new(4, -25, 0x89c8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a4a)]),
        (0xad82, [new(4, -25, 0x89c8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a5a)]),
        (0xada4, [new(4, -25, 0x89c8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a6a)]),
        (0xadc6, [new(15, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88), new(4, -25, 0x8a18)]),
        (0xade8, [new(15, -31, 0x87c7), new(4, -25, 0x89d8), new(0, 0, 0x8a88), new(4, -25, 0x8a18)]),
        (0xae0a, [new(15, -31, 0x87c7), new(4, -25, 0x89e8), new(0, 0, 0x8a88), new(4, -25, 0x8a18)]),
        (0xae2c, [new(4, -25, 0x89f8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a18)]),
        (0xae4e, [new(4, -25, 0x8a08), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a18)]),
        (0xae70, [new(15, -31, 0x87c7), new(4, -25, 0x89c8), new(0, 0, 0x8a88)]),
        (0xae8a, [new(15, -31, 0x87c7), new(4, -25, 0x89d8), new(0, 0, 0x8a88)]),
        (0xaea4, [new(4, -25, 0x89e8), new(15, -31, 0x87c7), new(0, 0, 0x8a88)]),
        (0xaebe, [new(4, -25, 0x89f8), new(15, -31, 0x87c7), new(0, 0, 0x8a88)]),
        (0xaed8, [new(4, -25, 0x8a08), new(15, -31, 0x87c7), new(0, 0, 0x8a88)]),
        (0xaef2, [new(15, -31, 0x87c7), new(4, -25, 0x89b8), new(0, 0, 0x8a88), new(4, -25, 0x8a2a)]),
        (0xaf14, [new(15, -31, 0x87c7), new(4, -25, 0x89b8), new(0, 0, 0x8a88), new(4, -25, 0x8a3a)]),
        (0xaf36, [new(15, -31, 0x87c7), new(4, -25, 0x89b8), new(0, 0, 0x8a88), new(4, -25, 0x8a4a)]),
        (0xaf58, [new(4, -25, 0x89b8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a5a)]),
        (0xaf7a, [new(4, -25, 0x89b8), new(15, -31, 0x87c7), new(0, 0, 0x8a88), new(4, -25, 0x8a6a)]),
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
            0x87c7 or 0x89b8 or 0x89c8 or 0x89d8 or 0x89e8 or 0x89f8 or
            0x8a08 or 0x8a18 or 0x8a2a or 0x8a3a or 0x8a4a or 0x8a5a or
            0x8a6a => [],
            _ => throw new InvalidDataException(
                $"Golden Torizo right sonic hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
