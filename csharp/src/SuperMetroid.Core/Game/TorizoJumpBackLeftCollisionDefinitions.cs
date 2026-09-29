namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical components for the three shared left-facing Torizo backward-jump
/// frames at $AA:AFFA/B014/B02E. The component geometry and $AA:891C body
/// hitbox remain cartridge mechanics even if their OAM art is replaced.
/// </summary>
internal static class TorizoJumpBackLeftCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xaffa, [new(-16, -29, 0x87c7), new(-4, -22, 0x885a), new(0, 0, 0x891c)]),
        (0xb014, [new(-16, -30, 0x87c7), new(-4, -24, 0x8858), new(0, 0, 0x891c)]),
        (0xb02e, [new(-16, -30, 0x87c7), new(-3, -24, 0x8858), new(0, 0, 0x891c)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] Body =
        [new(-18, -38, 7, 9, 0xc977, 0xc97c)];

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
            0x891c => Body,
            0x87c7 or 0x8858 or 0x885a => [],
            _ => throw new InvalidDataException(
                $"Torizo left jump-back hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
