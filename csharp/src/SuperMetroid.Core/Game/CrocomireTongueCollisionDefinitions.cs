namespace SuperMetroid.Core.Game;

/// <summary>Native physical component of a selected Crocomire tongue frame.</summary>
internal readonly record struct CrocomireTongueCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>
/// All nine extended frames selected by Crocomire's independently scheduled
/// tongue actor. The four fight frames refer to the empty $A4:CBB3 hitbox
/// list; the five melting frames refer to the empty $A4:CC3B list. Their OAM
/// pointers are deliberately absent so presentation can be edited separately.
/// </summary>
internal static class CrocomireTongueCollisionDefinitions
{
    /// <summary>Native bank containing the selected Crocomire extended frames.</summary>
    internal const byte Bank = 0xa4;

    private static readonly ushort[] FrameKeys =
    [
        0xc65e, 0xc668, 0xc672, 0xc67c,
        0xcace, 0xcad8, 0xcae2, 0xcaec, 0xcaf6,
    ];
    private static readonly Dictionary<ushort, CrocomireTongueCollisionComponent> Frames = new()
    {
        [0xc65e] = new(-32, -24, 0xcbb3),
        [0xc668] = new(-32, -24, 0xcbb3),
        [0xc672] = new(-32, -24, 0xcbb3),
        [0xc67c] = new(-32, -24, 0xcbb3),
        [0xcace] = new(1, 11, 0xcc3b),
        [0xcad8] = new(0, 8, 0xcc3b),
        [0xcae2] = new(1, 8, 0xcc3b),
        [0xcaec] = new(0, 10, 0xcc3b),
        [0xcaf6] = new(1, 12, 0xcc3b),
    };

    internal static ReadOnlySpan<ushort> FramePointers => FrameKeys;
    internal static bool HasFrame(ushort frame) => Frames.ContainsKey(frame);

    internal static CrocomireTongueCollisionComponent ComponentAt(ushort frame) =>
        Frames.TryGetValue(frame, out CrocomireTongueCollisionComponent component)
            ? component
            : throw new InvalidDataException(
                $"Crocomire tongue frame $A4:{frame:X4} has no compiled collision.");

    internal static int HitboxCountAt(ushort list) => list switch
    {
        0xcbb3 or 0xcc3b => 0,
        _ => throw new InvalidDataException(
            $"Crocomire tongue hitbox list $A4:{list:X4} is not compiled."),
    };
}
