namespace SuperMetroid.Core.Game;

/// <summary>Physical component of one Golden Torizo extended frame; not editable art.</summary>
internal readonly record struct GoldenTorizoCollisionComponent(
    short X, short Y, ushort HitboxList);

/// <summary>Native ordered touch/shot rectangle from a bank-$AA hitbox list.</summary>
internal readonly record struct GoldenTorizoCollisionHitbox(
    short Left, short Top, short Right, short Bottom,
    ushort TouchAi, ushort ShotAi);

/// <summary>
/// Engine-owned component offsets and hitboxes for Golden Torizo's seated,
/// falling, and standing-up frames at $AA:AA12-AA5E. The matching OAM frame
/// compositions are separately installed as replaceable presentation art.
/// </summary>
internal static class GoldenTorizoAwakeningCollisionDefinitions
{
    internal const byte Bank = 0xaa;

    private static readonly (ushort Pointer, GoldenTorizoCollisionComponent[] Components)[] Frames =
    [
        (0xaa12, [new(0, 0, 0x87e8)]),
        (0xaa1c, [new(0, 0, 0x87f6)]),
        (0xaa26, [new(0, 0, 0x8804)]),
        (0xaa30, [new(0, 0, 0x8812)]),
        (0xaa3a, [new(-5, -24, 0x885a), new(0, 0, 0x8820)]),
        (0xaa4c, [new(-5, -24, 0x886a), new(0, 0, 0x882e)]),
        (0xaa5e, [new(-5, -24, 0x886a), new(0, 0, 0x883c)]),
    ];

    private static readonly GoldenTorizoCollisionHitbox[] SeatedLow =
        [new(-16, -27, 16, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] SeatedMid =
        [new(-14, -27, 13, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] SeatedHigh =
        [new(-13, -34, 9, 33, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] Initial =
        [new(-11, -38, 11, 39, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] Rising =
        [new(-15, -44, 8, 47, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] StandingMid =
        [new(-18, -43, 3, 24, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] StandingHigh =
        [new(-17, -42, 5, 15, 0xc977, 0xc9c2)];

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

    internal static ReadOnlySpan<GoldenTorizoCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x87e8 => SeatedLow,
            0x87f6 => SeatedMid,
            0x8804 => SeatedHigh,
            0x8812 => Initial,
            0x885a or 0x886a => [],
            0x8820 => Rising,
            0x882e => StandingMid,
            0x883c => StandingHigh,
            _ => throw new InvalidDataException(
                $"Golden Torizo hitbox list $AA:{pointer:X4} is not compiled."),
        };
}
