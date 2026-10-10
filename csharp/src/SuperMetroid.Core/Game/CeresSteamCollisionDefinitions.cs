namespace SuperMetroid.Core.Game;

/// <summary>Immutable physical component of a Ceres steam extended frame.</summary>
/// <param name="X">Horizontal pixel offset of this plume component from the steam origin.</param>
/// <param name="Y">Vertical pixel offset of this plume component from the steam origin.</param>
/// <param name="HitboxPointer">Bank-$A6 pointer to the collision rectangles assigned to this component.</param>
internal readonly record struct CeresSteamCollisionComponent(short X, short Y, ushort HitboxPointer);

/// <summary>Immutable steam rectangle and native touch/shot callback identities.</summary>
/// <param name="Left">Left edge of the rectangle relative to the steam origin, in pixels.</param>
/// <param name="Top">Top edge of the rectangle relative to the steam origin, in pixels.</param>
/// <param name="Right">Right edge of the rectangle relative to the steam origin, in pixels.</param>
/// <param name="Bottom">Bottom edge of the rectangle relative to the steam origin, in pixels.</param>
/// <param name="TouchAi">Native touch-collision routine selected for the rectangle.</param>
/// <param name="ShotAi">Native projectile-collision routine selected for the rectangle.</param>
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

    /// <summary>Native callback stored in the touch-AI word of each damaging steam rectangle.</summary>
    private const ushort Touch = EnemyAiCodePointers.BankA6.CeresSteamTouch;
    /// <summary>Native no-op callback stored in the shot-AI word of each steam rectangle.</summary>
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

    /// <summary>Ordered pointers for all 28 extended frames, including the hidden end frames.</summary>
    internal static FrameSequence FramePointers { get; } = new();

    /// <summary>Checks whether a pointer names one of the 28 ten-byte Ceres steam extended frames.</summary>
    /// <param name="frame">Candidate bank-$A6 extended-frame pointer.</param>
    internal static bool HasFrame(ushort frame) =>
        frame >= FirstFrame && frame < FirstFrame + 280 && (frame - FirstFrame) % 10 == 0;

    /// <summary>Maps a valid extended frame to its single zero-offset component and directional hitbox list.</summary>
    /// <param name="frame">Native frame pointer to resolve.</param>
    /// <returns>The one component containing the hitbox-list pointer used by the frame.</returns>
    /// <exception cref="InvalidDataException">The pointer is not one of the compiled steam frames.</exception>
    internal static ComponentSequence ComponentsAt(ushort frame)
    {
        if (!HasFrame(frame))
            throw new InvalidDataException($"Ceres steam frame $A6:{frame:X4} has no compiled collision.");
        int index = (frame - FirstFrame) / 10;
        int direction = index / 7;
        int pose = index % 7;
        return new(pose >= 5 ? EmptyHitboxes : (ushort)(FirstHitboxes + 14 * (5 * direction + pose)));
    }

    /// <summary>Enumerates all steam frame pointers in the cartridge's ten-byte record order.</summary>
    internal sealed class FrameSequence : IReadOnlyList<ushort>
    {
        /// <summary>Gets the number of compiled steam frames, including hidden frames.</summary>
        public int Count => 28;
        /// <summary>Gets the fixed number of frame pointers for sequence consumers.</summary>
        public int Length => Count;
        /// <summary>Gets the frame pointer at a zero-based position in the native record run.</summary>
        /// <param name="index">Zero-based frame position.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside the 28 compiled frames.</exception>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + 10 * index) : throw new IndexOutOfRangeException();
        /// <summary>Creates an iterator over frame pointers in cartridge order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Single-component sequence backed by the hitbox list selected for one steam frame.</summary>
    /// <param name="hitboxPointer">Bank-$A6 list pointer carried by the frame's component.</param>
    internal readonly struct ComponentSequence(ushort hitboxPointer) : IReadOnlyList<CeresSteamCollisionComponent>
    {
        /// <summary>Gets the one physical component present in every compiled steam frame.</summary>
        public int Count => 1;
        /// <summary>Gets the frame's zero-offset component at index zero.</summary>
        /// <param name="index">The only valid index is zero.</param>
        /// <exception cref="IndexOutOfRangeException">The index is not zero.</exception>
        public CeresSteamCollisionComponent this[int index] => index == 0
            ? new(0, 0, hitboxPointer) : throw new IndexOutOfRangeException();
        /// <summary>Creates an iterator that yields the frame's single collision component.</summary>
        public IEnumerator<CeresSteamCollisionComponent> GetEnumerator()
        {
            yield return this[0];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Returns the compiled rectangles for a native steam hitbox-list pointer.</summary>
    /// <param name="list">Bank-$A6 address of a steam hitbox list.</param>
    /// <returns>The list's rectangles, or an empty span for the shared hidden-frame list.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a compiled steam list.</exception>
    internal static ReadOnlySpan<CeresSteamCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGet(list, out CeresSteamCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Ceres steam hitbox list $A6:{list:X4} is not compiled.");

}
