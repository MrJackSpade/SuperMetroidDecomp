namespace SuperMetroid.Core.Game;

/// <summary>One immutable Oum rectangle with its native touch and shot callbacks.</summary>
internal readonly record struct MaridiaLargeSnailCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// Physical collision for Oum's 30 bank-$A2 extended frames. Every native frame
/// at $CB87-$CCA9 has exactly one component at (0,0); only its hitbox-list
/// pointer changes. The editable OAM and the three callback identities are
/// intentionally separate from this catalog.
/// </summary>
internal static class MaridiaLargeSnailCollisionDefinitions
{
    /// <summary>Native bank containing Oum's extended frames and hitbox lists.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>First Oum extended frame, <c>ExtendedSpritemap_Oum_FacingLeft_0</c> at $A2:CB87.</summary>
    private const ushort FirstFrame = 0xcb87;
    /// <summary>Last Oum extended frame, <c>ExtendedSpritemap_Oum_FacingRight_E</c> at $A2:CCA9.</summary>
    private const ushort LastFrame = 0xcca9;
    /// <summary>Each native one-component extended frame occupies ten bytes.</summary>
    private const int FrameStride = 10;
    private const ushort DamageTouch = EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch;
    private const ushort SafeTouch = EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot;
    private const ushort NoShot = EnemyAiCodePointers.BankA0.NoOp;

    /// <summary>First consecutive hitbox record, Hitbox_Oum_FacingLeft_0 at $A2:D034.</summary>
    private const ushort FirstHitboxList = 0xd034;

    /// <summary>
    /// The 30 contiguous hitbox lists from $A2:D034, one per frame in frame order. List
    /// addresses derive from their order; the rectangles are fitted to the drawings.
    /// </summary>
    private static readonly CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox> Lists = CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox>.HitboxLists(
        new CollisionRecordRun<MaridiaLargeSnailCollisionHitbox>(FirstHitboxList,
        [
            [new(-16, -17, -8, 16, SafeTouch, Shot), new(-8, -17, 14, 16, SafeTouch, NoShot)],
            [new(-16, -17, 14, 16, SafeTouch, NoShot)],
            [new(-1, -17, 14, 16, SafeTouch, NoShot), new(-17, -17, 0, 16, SafeTouch, Shot)],
            [new(-20, -8, 0, 8, DamageTouch, Shot), new(0, -17, 13, 16, DamageTouch, NoShot)],
            [new(-22, -8, 0, 7, DamageTouch, Shot), new(0, -17, 14, 16, DamageTouch, NoShot)],
            [new(-25, -9, 0, 8, DamageTouch, Shot), new(0, -18, 14, 16, DamageTouch, NoShot)],
            [new(-24, -8, 0, 9, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
            [new(-27, -8, 0, 8, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
            [new(-16, 0, 0, 16, SafeTouch, Shot), new(-16, -16, 0, 0, SafeTouch, NoShot), new(0, -16, 14, 16, SafeTouch, NoShot)],
            [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
            [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, NoShot), new(0, 0, 15, 16, SafeTouch, Shot)],
            [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 16, SafeTouch, Shot)],
            [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, Shot), new(0, 0, 15, 16, SafeTouch, NoShot)],
            [new(-16, -18, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
            [new(-16, 0, 0, 16, SafeTouch, NoShot), new(-16, -17, 0, 0, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
            [new(-16, -17, 8, 16, SafeTouch, NoShot), new(8, -17, 16, 16, SafeTouch, Shot)],
            [new(-16, -17, 16, 16, SafeTouch, NoShot)],
            [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 16, SafeTouch, Shot)],
            [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 20, 8, DamageTouch, Shot)],
            [new(-16, -17, -1, 16, DamageTouch, NoShot), new(0, -8, 22, 8, DamageTouch, Shot)],
            [new(-16, -18, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
            [new(-15, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
            [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 25, 8, DamageTouch, Shot)],
            [new(-15, -16, 0, 16, SafeTouch, NoShot), new(0, -16, 16, 0, SafeTouch, NoShot), new(0, 0, 16, 16, SafeTouch, Shot)],
            [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
            [new(-16, 0, 0, 17, SafeTouch, Shot), new(-16, -17, 0, 0, SafeTouch, NoShot), new(0, -17, 14, 17, SafeTouch, NoShot)],
            [new(-16, -17, 0, 16, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
            [new(-16, -17, 0, 0, SafeTouch, Shot), new(-16, 0, 0, 16, SafeTouch, NoShot), new(0, -17, 14, 16, SafeTouch, NoShot)],
            [new(-16, -17, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
            [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 0, SafeTouch, Shot), new(0, 0, 16, 16, SafeTouch, NoShot)],
        ]));

    internal static FramePointerSequence FramePointers => default;
    internal static IEnumerable<ushort> HitboxPointers => Lists.Pointers;

    internal static bool HasFrame(ushort frame) =>
        frame >= FirstFrame && frame <= LastFrame &&
        (frame - FirstFrame) % FrameStride == 0;

    internal static ushort HitboxListAt(ushort frame)
    {
        if (!HasFrame(frame))
            throw new InvalidDataException($"Oum frame $A2:{frame:X4} has no compiled collision.");
        return Lists.PointerAt((frame - FirstFrame) / FrameStride);
    }

    internal static ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGet(list, out MaridiaLargeSnailCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Oum hitbox list $A2:{list:X4} is not compiled.");

    /// <summary>Consecutive ten-byte one-component frame identities at $A2:CB87-$CCA9.</summary>
    internal readonly struct FramePointerSequence : IReadOnlyList<ushort>
    {
        public int Count => (LastFrame - FirstFrame) / FrameStride + 1;
        public int Length => Count;
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + index * FrameStride)
            : throw new IndexOutOfRangeException();
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
