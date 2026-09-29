namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's physical foot components for the initial frame at $A7:A565 and
/// the 35 selected extended frames at $A7:8CE3-$A7:8F47. The OAM records are presentation;
/// these offsets and the shared $A7:9453 hitbox list govern collision.
/// </summary>
internal static class KraidFootCollisionDefinitions
{
    internal const byte Bank = 0xa7;

    /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
    internal const ushort FirstFrame = 0x8ce3;

    /// <summary>Small-Kraid initial foot frame at $A7:A565; physically the first walk frame.</summary>
    internal const ushort InitialFrame = 0xa565;

    /// <summary>The shared single-rectangle hitbox list at $A7:9453.</summary>
    internal const ushort HitboxList = 0x9453;

    /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
    private const int FrameByteCount = 18;

    // Each row is the signed X/Y placement of the first and second component.
    // Both components retain their native shared hitbox-list pointer regardless
    // of future edits to their displayed OAM offsets.
    private static readonly (short Ax, short Ay, short Bx, short By)[] Offsets =
    [
        (8, 40, 0, 0), (6, 39, -2, -1), (4, 38, -4, -2),
        (2, 37, -6, -3), (0, 36, -8, -4), (-2, 35, 5, -15),
        (-4, 34, 3, -16), (-6, 33, 1, -17), (-8, 32, -1, -18),
        (-10, 31, -3, -19), (-12, 29, 12, -26), (-14, 28, 26, -23),
        (-16, 30, 24, -21), (-18, 32, 22, -19), (-20, 34, 20, -17),
        (-22, 36, 18, -15), (-24, 38, 16, -13), (-26, 40, 14, -11),
        (-24, 40, 16, -10), (-22, 40, 18, -10), (-20, 40, 20, -10),
        (-18, 40, 6, -15), (-16, 40, 8, -15), (-14, 40, -7, -10),
        (-12, 40, -5, -10), (-10, 40, -3, -10), (-8, 40, -1, -10),
        (-6, 40, 1, -10), (-4, 40, 3, -10), (-2, 40, 5, -10),
        (0, 40, 7, -10), (2, 40, -6, 0), (4, 40, -4, 0),
        (6, 40, -2, 0), (8, 40, 0, 0),
    ];

    private static readonly KraidFootCollisionComponent[][] Frames = CreateFrames();

    private static readonly KraidFootCollisionHitbox[] SharedHitboxes =
        [new(-6, -6, 6, 6, EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
            EnemyAiCodePointers.BankA7.KraidNoOpShot)];

    internal static int FrameCount => Frames.Length;

    internal static ushort FramePointer(int index) =>
        checked((ushort)(FirstFrame + index * FrameByteCount));

    internal static bool TryGetComponents(ushort pointer,
        out ReadOnlyMemory<KraidFootCollisionComponent> components)
    {
        if (pointer == InitialFrame)
        {
            components = Frames[0];
            return true;
        }

        int distance = pointer - FirstFrame;
        if (distance < 0 || distance % FrameByteCount != 0 ||
            distance / FrameByteCount >= Frames.Length)
        {
            components = default;
            return false;
        }

        components = Frames[distance / FrameByteCount];
        return true;
    }

    internal static ReadOnlySpan<KraidFootCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer == HitboxList
            ? SharedHitboxes
            : throw new InvalidDataException(
                $"Kraid-foot hitbox list $A7:{pointer:X4} is not compiled.");

    private static KraidFootCollisionComponent[][] CreateFrames()
    {
        var frames = new KraidFootCollisionComponent[Offsets.Length][];
        for (int index = 0; index < Offsets.Length; index++)
        {
            var offset = Offsets[index];
            frames[index] =
            [
                new(offset.Ax, offset.Ay, HitboxList),
                new(offset.Bx, offset.By, HitboxList),
            ];
        }
        return frames;
    }
}

internal readonly record struct KraidFootCollisionComponent(short X, short Y, ushort HitboxPointer);

internal readonly record struct KraidFootCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
