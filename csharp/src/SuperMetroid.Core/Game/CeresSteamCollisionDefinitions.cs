namespace SuperMetroid.Core.Game;

/// <summary>Immutable physical component of a Ceres steam extended frame.</summary>
internal readonly record struct CeresSteamCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Immutable steam rectangle and native touch/shot callback identities.</summary>
internal readonly record struct CeresSteamCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Bank-$A6 physical collision for all four Ceres steam directions. Each of the
/// 28 extended frames has one zero-offset component. Its OAM pointer is deliberately
/// absent: installed sprite edits cannot change the damaging plume geometry.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Native bank of the extended maps at $A6:F142-$F250.</summary>
    internal const byte Bank = 0xa6;

    private const ushort Touch = EnemyAiCodePointers.BankA6.CeresSteamTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA0.NoOp;

    /// <summary>First ten-byte extended frame, ExtendedSpritemap_CeresSteam_Up_0 at $A6:F142.</summary>
    private const ushort FirstFrame = 0xf142;
    /// <summary>Shared empty hitbox list at $A6:F25A, selected by both hidden frames.</summary>
    private const ushort EmptyHitboxes = 0xf25a;
    /// <summary>First fourteen-byte damaging rectangle, Hitbox_CeresSteam_Up_0 at $A6:F25C.</summary>
    private const ushort FirstHitboxes = 0xf25c;
    /// <summary>
    /// The 21 contiguous hitbox lists from $A6:F25A (the shared empty list, then five plume
    /// rectangles per direction). List addresses derive from their order; each direction's
    /// rectangles are fitted to its drawing and are not exact rotations of another direction.
    /// </summary>
    private static readonly CollisionRecordRuns<CeresSteamCollisionHitbox> Lists = CollisionRecordRuns<CeresSteamCollisionHitbox>.HitboxLists(
        new CollisionRecordRun<CeresSteamCollisionHitbox>(EmptyHitboxes,
        [
            [],
            [new(-8, -16, 7, -1, Touch, Shot)],
            [new(-8, -23, 7, -2, Touch, Shot)],
            [new(-8, -32, 7, -8, Touch, Shot)],
            [new(-8, -40, 7, -16, Touch, Shot)],
            [new(-8, -40, 6, -24, Touch, Shot)],
            [new(-16, -8, -1, 7, Touch, Shot)],
            [new(-24, -8, -2, 7, Touch, Shot)],
            [new(-32, -7, -9, 7, Touch, Shot)],
            [new(-40, -9, -17, 5, Touch, Shot)],
            [new(-40, -11, -26, 2, Touch, Shot)],
            [new(-8, 0, 7, 14, Touch, Shot)],
            [new(-8, 0, 7, 23, Touch, Shot)],
            [new(-8, 8, 7, 31, Touch, Shot)],
            [new(-8, 15, 7, 39, Touch, Shot)],
            [new(-8, 23, 6, 38, Touch, Shot)],
            [new(0, -8, 15, 7, Touch, Shot)],
            [new(1, -8, 23, 7, Touch, Shot)],
            [new(9, -8, 31, 7, Touch, Shot)],
            [new(18, -9, 38, 5, Touch, Shot)],
            [new(25, -11, 40, 3, Touch, Shot)],
        ]));

    internal static FrameSequence FramePointers { get; } = new();
    internal static bool HasFrame(ushort frame) =>
        frame >= FirstFrame && frame < FirstFrame + 280 && (frame - FirstFrame) % 10 == 0;

    internal static ComponentSequence ComponentsAt(ushort frame)
    {
        if (!HasFrame(frame))
            throw new InvalidDataException($"Ceres steam frame $A6:{frame:X4} has no compiled collision.");
        int index = (frame - FirstFrame) / 10;
        int direction = index / 7;
        int pose = index % 7;
        return new(pose >= 5 ? EmptyHitboxes : (ushort)(FirstHitboxes + 14 * (5 * direction + pose)));
    }

    internal sealed class FrameSequence : IReadOnlyList<ushort>
    {
        public int Count => 28;
        public int Length => Count;
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + 10 * index) : throw new IndexOutOfRangeException();
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal readonly struct ComponentSequence(ushort hitboxPointer) : IReadOnlyList<CeresSteamCollisionComponent>
    {
        public int Count => 1;
        public CeresSteamCollisionComponent this[int index] => index == 0
            ? new(0, 0, hitboxPointer) : throw new IndexOutOfRangeException();
        public IEnumerator<CeresSteamCollisionComponent> GetEnumerator()
        {
            yield return this[0];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal static ReadOnlySpan<CeresSteamCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGet(list, out CeresSteamCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Ceres steam hitbox list $A6:{list:X4} is not compiled.");

}
