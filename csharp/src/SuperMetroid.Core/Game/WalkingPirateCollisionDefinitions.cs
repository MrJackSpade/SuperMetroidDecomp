// Generated from the pinned cartridge by --generate-walking-pirate-collision.
namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox identity in one extended frame component.</summary>
internal readonly record struct WalkingPirateCollisionComponent(short X, short Y, ushort HitboxPointer);
/// <summary>Engine-owned signed collision bounds and touch/shot callbacks.</summary>
internal readonly record struct WalkingPirateCollisionHitbox(short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
internal readonly record struct WalkingPirateCollisionFrame(ushort Pointer, WalkingPirateCollisionComponent[] Components);
internal readonly record struct WalkingPirateCollisionList(ushort Pointer, WalkingPirateCollisionHitbox[] Rectangles);

/// <summary>Fixed bank-$B2 walking-Pirate collision data, separate from editable OAM art.</summary>
internal static class WalkingPirateCollisionDefinitions
{
    private static readonly WalkingPirateCollisionFrame[] Frames =
    [
        new(0x804F, [new(0, 0, 0x8059), ]),
        new(0x89C4, [new(-5, 3, 0x9B60), new(0, 0, 0x9826), ]),
        new(0x89D6, [new(-5, 3, 0x9B7C), new(0, 0, 0x9834), ]),
        new(0x89E8, [new(-5, 3, 0x9B8A), new(0, 0, 0x9842), ]),
        new(0x89FA, [new(-5, 3, 0x9B98), new(2, 0, 0x9850), ]),
        new(0x8A0C, [new(-5, 3, 0x9B98), new(2, 0, 0x985E), ]),
        new(0x8A1E, [new(-5, 3, 0x9B8A), new(2, 0, 0x986C), ]),
        new(0x8A30, [new(-5, 3, 0x9B7C), new(0, 0, 0x987A), ]),
        new(0x8A42, [new(-5, 3, 0x9B60), new(0, 0, 0x9888), ]),
        new(0x8AF6, [new(5, 3, 0x9D5E), new(0, 0, 0x994C), ]),
        new(0x8B08, [new(5, 3, 0x9D7A), new(0, 0, 0x995A), ]),
        new(0x8B1A, [new(5, 3, 0x9D88), new(0, 0, 0x9968), ]),
        new(0x8B2C, [new(5, 3, 0x9D96), new(0, 0, 0x9976), ]),
        new(0x8B3E, [new(5, 3, 0x9D96), new(-1, 0, 0x9984), ]),
        new(0x8B50, [new(5, 3, 0x9D88), new(0, 0, 0x9992), ]),
        new(0x8B62, [new(5, 3, 0x9D7A), new(1, 0, 0x99A0), ]),
        new(0x8B74, [new(5, 3, 0x9D5E), new(1, 0, 0x99AE), ]),
        new(0x8C28, [new(0, 3, 0x9A48), new(0, 3, 0x9896), ]),
        new(0x8C3A, [new(0, 3, 0x9A56), new(0, 3, 0x9896), ]),
        new(0x8C4C, [new(0, 3, 0x9A64), new(0, 3, 0x9896), ]),
        new(0x8C5E, [new(0, 3, 0x9A72), new(0, 3, 0x9896), ]),
        new(0x8C70, [new(-1, 4, 0x9A80), new(0, 3, 0x9896), ]),
        new(0x8C82, [new(-2, 6, 0x9A8E), new(0, 3, 0x9922), ]),
        new(0x8C94, [new(0, 3, 0x9A9C), new(0, 3, 0x99BC), ]),
        new(0x8CA6, [new(0, 3, 0x9AAA), new(0, 3, 0x99BC), ]),
        new(0x8CB8, [new(0, 3, 0x9AB8), new(0, 3, 0x99BC), ]),
        new(0x8CCA, [new(0, 3, 0x9AC6), new(0, 3, 0x99BC), ]),
        new(0x8CDC, [new(1, 4, 0x9AD4), new(0, 3, 0x99BC), ]),
        new(0x8CEE, [new(2, 6, 0x9AE2), new(0, 3, 0x9A3A), ]),
        new(0x8D00, [new(-5, -12, 0x9A48), new(0, 3, 0x9930), new(0, 3, 0x9896), ]),
        new(0x8D1A, [new(0, 3, 0x9A48), new(0, 3, 0x9896), ]),
        new(0x8D2C, [new(-5, -11, 0x9A48), new(0, 3, 0x9930), new(0, 3, 0x9896), ]),
        new(0x8D46, [new(5, -12, 0x9A48), new(0, 3, 0x993E), new(0, 3, 0x99BC), ]),
        new(0x8D60, [new(0, 3, 0x9A9C), new(0, 3, 0x99BC), ]),
        new(0x8D72, [new(5, -11, 0x9A48), new(0, 3, 0x993E), new(0, 3, 0x99BC), ]),
        new(0x8D8C, [new(0, 1, 0x971C), ]),
        new(0x8F92, [new(0, 8, 0x9C78), ]),
        new(0x8FA6, [new(0, 8, 0x9E68), ]),
    ];
    private static readonly WalkingPirateCollisionList[] Lists =
    [
        new(0x8059, [new(0, 0, 0, 0, 0x8023, 0x802D), ]),
        new(0x971C, [new(-13, -19, 10, 30, 0x876C, 0x8779), ]),
        new(0x9826, [new(-11, 0, 8, 30, 0x876C, 0x8779), ]),
        new(0x9834, [new(-11, 0, 8, 30, 0x876C, 0x8779), ]),
        new(0x9842, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9850, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x985E, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x986C, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x987A, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9888, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9896, [new(-7, 0, 6, 30, 0x876C, 0x883E), ]),
        new(0x9922, [new(-7, 0, 6, 30, 0x876C, 0x883E), ]),
        new(0x9930, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x993E, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x994C, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x995A, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9968, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9976, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x9984, [new(-7, -1, 6, 30, 0x876C, 0x8779), ]),
        new(0x9992, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x99A0, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x99AE, [new(-7, 0, 6, 30, 0x876C, 0x8779), ]),
        new(0x99BC, [new(-7, 0, 6, 30, 0x876C, 0x883E), ]),
        new(0x9A3A, [new(-7, 0, 6, 30, 0x876C, 0x883E), ]),
        new(0x9A48, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A56, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A64, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A72, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A80, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A8E, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9A9C, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9AAA, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9AB8, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9AC6, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9AD4, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9AE2, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9B60, [new(-7, -19, 6, 0, 0x876C, 0x87C8), ]),
        new(0x9B7C, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9B8A, [new(-7, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x9B98, [new(-7, -19, 6, 0, 0x876C, 0x883E), ]),
        new(0x9C78, [new(-7, -19, 6, 23, 0x876C, 0x8779), new(-18, -18, -7, 2, 0x876C, 0x87C8), ]),
        new(0x9D5E, [new(-7, -19, 6, 0, 0x876C, 0x87C8), ]),
        new(0x9D7A, [new(-7, -19, 6, 0, 0x876C, 0x87C8), ]),
        new(0x9D88, [new(-7, -19, 6, 0, 0x876C, 0x87C8), ]),
        new(0x9D96, [new(-7, -19, 6, 0, 0x876C, 0x87C8), ]),
        new(0x9E68, [new(-7, -19, 6, 23, 0x876C, 0x8779), new(6, -19, 17, 1, 0x876C, 0x87C8), ]),
    ];
    internal static int FrameCount => Frames.Length;
    internal static int ListCount => Lists.Length;
    internal static WalkingPirateCollisionFrame Frame(int index) => Frames[index];
    internal static WalkingPirateCollisionList List(int index) => Lists[index];

    internal static ReadOnlySpan<WalkingPirateCollisionComponent> ComponentsAt(ushort pointer)
    {
        int low = 0, high = Frames.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ushort candidate = Frames[middle].Pointer;
            if (candidate == pointer) return Frames[middle].Components;
            if (candidate < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Walking Pirate collision frame $B2:{pointer:X4} is not compiled.");
    }
    internal static ReadOnlySpan<WalkingPirateCollisionHitbox> HitboxesAt(ushort pointer)
    {
        int low = 0, high = Lists.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ushort candidate = Lists[middle].Pointer;
            if (candidate == pointer) return Lists[middle].Rectangles;
            if (candidate < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Walking Pirate hitbox list $B2:{pointer:X4} is not compiled.");
    }
}
