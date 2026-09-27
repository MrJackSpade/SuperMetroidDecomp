namespace SuperMetroid.Core.Game;

/// <summary>Fixed collision identity for Draygon's ordinary bank-$A5 OAM frames.</summary>
internal static partial class DraygonCollisionDefinitions
{
    /// <summary>$A5:ABDD, the second empty hitbox list used by mirrored OAM frames.</summary>
    internal const ushort OtherEmptyList = 0xabdd;

    // The cartridge gives every component of these 48 frames either $AAC7
    // or $ABDD, both zero-record hitbox lists. Thus the native walker can
    // never select a callback, regardless of the component's visual offset.
    // The $EE/$EF frames elsewhere in the visual catalog are Spore Spawn,
    // not Draygon, and deliberately do not belong to this set.
    private static readonly HashSet<ushort> EmptyOamFrames =
    [
        0xa2df, 0xa2e9, 0xa2f3, 0xa2fd, 0xa307, 0xa311,
        0xa3c5, 0xa3cf, 0xa3d9, 0xa3e3,
        0xa40b, 0xa41d, 0xa42f, 0xa441, 0xa453, 0xa465,
        0xa477, 0xa489, 0xa4a3, 0xa4c5, 0xa4ef, 0xa521,
        0xa55b, 0xa59d,
        0xa607, 0xa611, 0xa61b, 0xa625, 0xa62f, 0xa639,
        0xa6ed, 0xa6f7, 0xa701, 0xa70b,
        0xa779, 0xa78b, 0xa79d, 0xa7af, 0xa7c1, 0xa7d3,
        0xa7e5, 0xa7f7, 0xa811, 0xa833, 0xa85d, 0xa88f,
        0xa8c9, 0xa90b,
    ];

    internal static bool IsEmptyOamFrame(ushort pointer) =>
        EmptyOamFrames.Contains(pointer);

    internal static int EmptyOamFrameCount => EmptyOamFrames.Count;

    private static ReadOnlySpan<DraygonCollisionComponent> OamComponentsAt(
        ushort pointer)
    {
        if (IsEmptyOamFrame(pointer))
            return [];
        throw new InvalidDataException(
            $"Draygon frame $A5:{pointer:X4} has no compiled collision identity.");
    }

    private static ReadOnlySpan<DraygonCollisionHitbox> OamHitboxesAt(
        ushort pointer)
    {
        if (pointer == OtherEmptyList)
            return [];
        throw new InvalidDataException(
            $"Draygon hitbox list $A5:{pointer:X4} is not compiled.");
    }
}
