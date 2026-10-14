namespace SuperMetroid.Core.Game;

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
    /// <summary>Bank-$A2 callback used by Oum's extended hitboxes that hurt Samus on contact.</summary>
    private const ushort DamageTouch = EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch;
    /// <summary>Bank-$A2 callback for Oum surfaces that permit harmless contact.</summary>
    private const ushort SafeTouch = EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch;
    /// <summary>Bank-$A2 callback for the shot-sensitive portion of an Oum frame.</summary>
    private const ushort Shot = EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot;
    /// <summary>Bank-$A0 no-op callback on Oum hitboxes that do not react to shots.</summary>
    private const ushort NoShot = EnemyAiCodePointers.BankA0.NoOp;

    /// <summary>First consecutive hitbox record, Hitbox_Oum_FacingLeft_0 at $A2:D034.</summary>
    private const ushort FirstHitboxList = 0xd034;

    /// <summary>
    /// The 30 contiguous hitbox lists from $A2:D034, one per frame in frame order. List
    /// addresses derive from their order; the rectangles are fitted to the drawings.
    /// </summary>
    internal static readonly CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox> Lists = CollisionRecordRuns<MaridiaLargeSnailCollisionHitbox>.HitboxLists(
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

    /// <summary>Enumerates all thirty one-component Oum frames in native address order.</summary>
    internal static FramePointerSequence FramePointers => default;

    /// <summary>Accepts only aligned ten-byte frame starts within Oum's compiled range.</summary>
    internal static bool HasFrame(ushort frame) =>
        frame >= FirstFrame && frame <= LastFrame &&
        (frame - FirstFrame) % FrameStride == 0;

    /// <summary>Selects the frame's corresponding hitbox list, rejecting other bank-$A2 frame pointers.</summary>
    internal static ushort HitboxListAt(ushort frame)
    {
        if (!HasFrame(frame))
            throw new InvalidDataException($"Oum frame $A2:{frame:X4} has no compiled collision.");
        return Lists.PointerAt((frame - FirstFrame) / FrameStride);
    }

    /// <summary>Resolves a compiled Oum hitbox-list pointer without reading editable presentation art.</summary>
    internal static ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGet(list, out MaridiaLargeSnailCollisionHitbox[] hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Oum hitbox list $A2:{list:X4} is not compiled.");

    /// <summary>Consecutive ten-byte one-component frame identities at $A2:CB87-$CCA9.</summary>
    internal readonly struct FramePointerSequence : IReadOnlyList<ushort>
    {
        /// <summary>Thirty native frames cover both facing directions.</summary>
        public int Count => (LastFrame - FirstFrame) / FrameStride + 1;
        /// <summary>Alias for the compiled frame count.</summary>
        public int Length => Count;
        /// <summary>Calculates an aligned frame pointer from its native ordinal.</summary>
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstFrame + index * FrameStride)
            : throw new IndexOutOfRangeException();
        /// <summary>Visits Oum frame pointers in ascending native address order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same frame ordering through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
