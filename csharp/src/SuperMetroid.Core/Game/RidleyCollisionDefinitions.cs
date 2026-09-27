namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned offset and hitbox-list identity of one Ridley body component.</summary>
internal readonly record struct RidleyCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Engine-owned rectangle and callbacks; never derived from editable OAM.</summary>
internal readonly record struct RidleyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// All eleven bank-$A6 extended body frames selected by Ceres and Lower Norfair
/// Ridley's instruction programs, and their seventeen distinct hitbox lists.
/// The original $A0:9A5A/$9B7F collision walker consumes these offsets and
/// rectangles independently of the displayed sprite composition.
/// </summary>
internal static class RidleyCollisionDefinitions
{
    /// <summary>$A6, shared native bank for both Ceres and Lower Norfair Ridley body maps.</summary>
    internal const byte Bank = 0xa6;
    private const ushort Touch = EnemyAiCodePointers.BankA6.RidleyExtendedTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA6.RidleyShot;

    private static readonly ushort[] FrameKeys =
    [
        0xe983, 0xe9a5, 0xe9c7, 0xe9e9, 0xea0b, 0xea2d,
        0xea4f, 0xea71, 0xea93, 0xeab5, 0xead7,
    ];
    private static readonly ushort[] ListKeys =
    [
        0xeae1, 0xeafb, 0xeb15, 0xeb2f, 0xeb3d, 0xeb4b,
        0xeb59, 0xeb67, 0xeb91, 0xebab, 0xebc5, 0xebdf,
        0xebf9, 0xec07, 0xec15, 0xec23, 0xec31,
    ];

    private static readonly RidleyCollisionComponent[] LeftBase =
    [new(15, 22, 0xeb2f), new(-8, 7, 0xeb59),
        new(16, 0, 0xeb67), new(-3, -24, 0xeae1)];
    private static readonly RidleyCollisionComponent[] RightBase =
    [new(-15, 22, 0xebf9), new(8, 7, 0xec23),
        new(-16, 0, 0xec31), new(3, -24, 0xebab)];

    private static readonly Dictionary<ushort, RidleyCollisionComponent[]> Frames = new()
    {
        [0xe983] = LeftBase,
        [0xe9a5] = RightBase,
        [0xe9c7] = [LeftBase[0], LeftBase[1], LeftBase[2], new(-3, -24, 0xeafb)],
        [0xe9e9] = [LeftBase[0], LeftBase[1], LeftBase[2], new(-3, -24, 0xeb15)],
        [0xea0b] = [RightBase[0], RightBase[1], RightBase[2], new(3, -24, 0xebc5)],
        [0xea2d] = [RightBase[0], RightBase[1], RightBase[2], new(3, -24, 0xebdf)],
        [0xea4f] = [new(15, 22, 0xeb3d), LeftBase[1], LeftBase[2], LeftBase[3]],
        [0xea71] = [new(15, 22, 0xeb4b), LeftBase[1], LeftBase[2], LeftBase[3]],
        [0xea93] = [new(-15, 22, 0xec07), RightBase[1], RightBase[2], RightBase[3]],
        [0xeab5] = [new(-15, 22, 0xec15), RightBase[1], RightBase[2], RightBase[3]],
        [0xead7] = [new(0, -6, 0xeb91)],
    };

    private static readonly Dictionary<ushort, RidleyCollisionHitbox[]> Lists = new()
    {
        [0xeae1] = [new(-12, -26, 11, 13, Touch, Shot), new(-24, 3, -13, 21, Touch, Shot)],
        [0xeafb] = [new(-41, -19, -21, -9, Touch, Shot), new(-20, -29, 11, 5, Touch, Shot)],
        [0xeb15] = [new(-37, -40, -14, -31, Touch, Shot), new(-25, -31, 9, 6, Touch, Shot)],
        [0xeb2f] = [new(-15, -10, 7, 2, Touch, Shot)],
        [0xeb3d] = [new(-17, -9, 6, 15, Touch, Shot)],
        [0xeb4b] = [new(-14, -1, 10, 23, Touch, Shot)],
        [0xeb59] = [new(-15, -2, -1, 8, Touch, Shot)],
        [0xeb67] = [new(-16, -20, 12, 21, Touch, Shot)],
        [0xeb91] = [new(-16, -32, 16, 34, Touch, Shot), new(-8, -45, 8, -33, Touch, Shot)],
        [0xebab] = [new(-12, -25, 11, 13, Touch, Shot), new(12, 5, 24, 20, Touch, Shot)],
        [0xebc5] = [new(-13, -29, 20, 5, Touch, Shot), new(21, -18, 39, -8, Touch, Shot)],
        [0xebdf] = [new(-10, -31, 25, 8, Touch, Shot), new(13, -42, 35, -32, Touch, Shot)],
        [0xebf9] = [new(-10, -10, 17, 2, Touch, Shot)],
        [0xec07] = [new(-9, -8, 17, 15, Touch, Shot)],
        [0xec15] = [new(-11, -8, 14, 23, Touch, Shot)],
        [0xec23] = [new(1, -2, 14, 9, Touch, Shot)],
        [0xec31] = [new(-13, -22, 14, 21, Touch, Shot)],
    };

    internal static ReadOnlySpan<ushort> FramePointers => FrameKeys;
    internal static ReadOnlySpan<ushort> HitboxPointers => ListKeys;
    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);

    internal static ReadOnlySpan<RidleyCollisionComponent> ComponentsAt(ushort frame) =>
        Frames.TryGetValue(frame, out RidleyCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Ridley frame $A6:{frame:X4} has no compiled collision.");

    internal static ReadOnlySpan<RidleyCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGetValue(list, out RidleyCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException($"Ridley hitbox list $A6:{list:X4} is not compiled.");
}
