// Generated from the pinned cartridge by --generate-space-pirate-collision.
namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox identity in one extended frame component.</summary>
internal readonly record struct SpacePirateCollisionComponent(short X, short Y, ushort HitboxPointer);
/// <summary>Engine-owned signed collision bounds and touch/shot callbacks.</summary>
internal readonly record struct SpacePirateCollisionHitbox(short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
internal readonly record struct SpacePirateCollisionFrame(ushort Pointer, SpacePirateCollisionComponent[] Components);
internal readonly record struct SpacePirateCollisionList(ushort Pointer, SpacePirateCollisionHitbox[] Rectangles);

/// <summary>Fixed bank-$B2 walking/wall-Pirate collision data, separate from editable OAM art.</summary>
internal static class SpacePirateCollisionDefinitions
{
    private static readonly SpacePirateCollisionFrame[] Frames =
    [
        new(0x804F, [new(0, 0, 0x8059), ]),
        new(0x88A0, [new(0, 0, 0x970E), new(0, 0, 0x9690), ]),
        new(0x88B2, [new(0, 0, 0x9700), new(0, 0, 0x969E), ]),
        new(0x88C4, [new(0, 0, 0x96F2), new(0, 0, 0x96AC), ]),
        new(0x88D6, [new(0, 0, 0x96BA), new(0, 0, 0x96E4), ]),
        new(0x88E8, [new(0, 0, 0x96C8), new(0, 0, 0x96D6), ]),
        new(0x88FA, [new(0, -2, 0x972A), new(0, 0, 0x970E), ]),
        new(0x890C, [new(1, -2, 0x9738), new(0, 0, 0x96D6), ]),
        new(0x891E, [new(0, 0, 0x9746), ]),
        new(0x8928, [new(0, 0, 0x9754), ]),
        new(0x8932, [new(0, 0, 0x97E0), new(0, 0, 0x9762), ]),
        new(0x8944, [new(0, 0, 0x97D2), new(0, 0, 0x9770), ]),
        new(0x8956, [new(0, 0, 0x97C4), new(0, 0, 0x977E), ]),
        new(0x8968, [new(0, 0, 0x978C), new(0, 0, 0x97B6), ]),
        new(0x897A, [new(0, 0, 0x979A), new(0, 0, 0x97A8), ]),
        new(0x898C, [new(0, 0, 0x97EE), new(0, 2, 0x97A8), ]),
        new(0x899E, [new(0, 0, 0x97FC), new(0, 2, 0x97A8), ]),
        new(0x89B0, [new(0, 0, 0x980A), ]),
        new(0x89BA, [new(0, 0, 0x9818), ]),
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
    private static readonly SpacePirateCollisionList[] Lists =
    [
        new(0x8059, [new(0, 0, 0, 0, 0x8023, 0x802D), ]),
        new(0x9690, [new(-18, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x969E, [new(-18, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x96AC, [new(-18, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x96BA, [new(-18, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x96C8, [new(-18, -19, 6, 0, 0x876C, 0x8779), ]),
        new(0x96D6, [new(-14, 0, 4, 30, 0x876C, 0x8779), ]),
        new(0x96E4, [new(-15, 0, -1, 30, 0x876C, 0x8779), ]),
        new(0x96F2, [new(-15, -6, 0, 23, 0x876C, 0x8779), ]),
        new(0x9700, [new(-16, -5, -1, 25, 0x876C, 0x8779), ]),
        new(0x970E, [new(-17, -8, 0, 30, 0x876C, 0x8779), ]),
        new(0x971C, [new(-13, -19, 10, 30, 0x876C, 0x8779), ]),
        new(0x972A, [new(-15, -19, 14, 6, 0x876C, 0x8779), ]),
        new(0x9738, [new(-16, -19, 14, 3, 0x876C, 0x8779), ]),
        new(0x9746, [new(-10, -21, 19, 22, 0x876C, 0x8779), ]),
        new(0x9754, [new(-8, -19, 18, 16, 0x876C, 0x8779), ]),
        new(0x9762, [new(-9, -23, 17, 0, 0x876C, 0x8779), ]),
        new(0x9770, [new(-9, -19, 16, 0, 0x876C, 0x8779), ]),
        new(0x977E, [new(-9, -19, 17, 0, 0x876C, 0x8779), ]),
        new(0x978C, [new(-9, -19, 16, 0, 0x876C, 0x8779), ]),
        new(0x979A, [new(-9, -19, 17, 0, 0x876C, 0x8779), ]),
        new(0x97A8, [new(-7, 0, 15, 30, 0x876C, 0x8779), ]),
        new(0x97B6, [new(-2, 0, 15, 30, 0x876C, 0x8779), ]),
        new(0x97C4, [new(-2, 0, 15, 23, 0x876C, 0x8779), ]),
        new(0x97D2, [new(0, 0, 15, 25, 0x876C, 0x8779), ]),
        new(0x97E0, [new(-1, 0, 15, 30, 0x876C, 0x8779), ]),
        new(0x97EE, [new(-15, -19, 15, 0, 0x876C, 0x8779), ]),
        new(0x97FC, [new(-15, -19, 14, 3, 0x876C, 0x8779), ]),
        new(0x980A, [new(-20, -19, 10, 25, 0x876C, 0x8779), ]),
        new(0x9818, [new(-20, -19, 6, 16, 0x876C, 0x8779), ]),
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
    internal static SpacePirateCollisionFrame Frame(int index) => Frames[index];
    internal static SpacePirateCollisionList List(int index) => Lists[index];

    internal static ReadOnlySpan<SpacePirateCollisionComponent> ComponentsAt(ushort pointer)
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
        throw new InvalidDataException($"Space Pirate collision frame $B2:{pointer:X4} is not compiled.");
    }
    internal static ReadOnlySpan<SpacePirateCollisionHitbox> HitboxesAt(ushort pointer)
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
        throw new InvalidDataException($"Space Pirate hitbox list $B2:{pointer:X4} is not compiled.");
    }
}
