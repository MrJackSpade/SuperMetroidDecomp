using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

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
    private const EnemyInteractionCallback DamageTouch = EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch;
    /// <summary>Bank-$A2 callback for Oum surfaces that permit harmless contact.</summary>
    private const EnemyInteractionCallback SafeTouch = EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch;
    /// <summary>Bank-$A2 callback for the shot-sensitive portion of an Oum frame.</summary>
    private const EnemyInteractionCallback Shot = EnemyInteractionCallback.MaridiaLargeSnailShot;
    /// <summary>Bank-$A0 no-op callback on Oum hitboxes that do not react to shots.</summary>
    private const EnemyInteractionCallback NoShot = EnemyInteractionCallback.NoOp;

    /// <summary>The 30 hitbox lists, one per frame in frame order; the rectangles are fitted to the drawings.</summary>
    internal static readonly MaridiaLargeSnailCollisionHitbox[][] Lists =
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
    ];

    /// <summary>Each of the 30 one-component Oum frames, whose single zero-offset component holds its hitbox list.</summary>
    internal static EnemyFrameCollision<MaridiaLargeSnailCollisionHitbox> Frames { get; } = new()
    {
        { EnemyFrameId.OumOamCB87, Lists[0] },
        { EnemyFrameId.OumOamCB91, Lists[1] },
        { EnemyFrameId.OumOamCB9B, Lists[2] },
        { EnemyFrameId.OumOamCBA5, Lists[3] },
        { EnemyFrameId.OumOamCBAF, Lists[4] },
        { EnemyFrameId.OumOamCBB9, Lists[5] },
        { EnemyFrameId.OumOamCBC3, Lists[6] },
        { EnemyFrameId.OumOamCBCD, Lists[7] },
        { EnemyFrameId.OumOamCBD7, Lists[8] },
        { EnemyFrameId.OumOamCBE1, Lists[9] },
        { EnemyFrameId.OumOamCBEB, Lists[10] },
        { EnemyFrameId.OumOamCBF5, Lists[11] },
        { EnemyFrameId.OumOamCBFF, Lists[12] },
        { EnemyFrameId.OumOamCC09, Lists[13] },
        { EnemyFrameId.OumOamCC13, Lists[14] },
        { EnemyFrameId.OumOamCC1D, Lists[15] },
        { EnemyFrameId.OumOamCC27, Lists[16] },
        { EnemyFrameId.OumOamCC31, Lists[17] },
        { EnemyFrameId.OumOamCC3B, Lists[18] },
        { EnemyFrameId.OumOamCC45, Lists[19] },
        { EnemyFrameId.OumOamCC4F, Lists[20] },
        { EnemyFrameId.OumOamCC59, Lists[21] },
        { EnemyFrameId.OumOamCC63, Lists[22] },
        { EnemyFrameId.OumOamCC6D, Lists[23] },
        { EnemyFrameId.OumOamCC77, Lists[24] },
        { EnemyFrameId.OumOamCC81, Lists[25] },
        { EnemyFrameId.OumOamCC8B, Lists[26] },
        { EnemyFrameId.OumOamCC95, Lists[27] },
        { EnemyFrameId.OumOamCC9F, Lists[28] },
        { EnemyFrameId.OumOamCCA9, Lists[29] },
    };

    /// <summary>Enumerates all thirty one-component Oum frames in native address order.</summary>
    internal static FramePointerSequence FramePointers => default;

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
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Visits Oum frame pointers in ascending native address order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same frame ordering through non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
