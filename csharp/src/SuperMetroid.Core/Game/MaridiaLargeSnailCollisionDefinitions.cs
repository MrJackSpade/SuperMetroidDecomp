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

    /// <summary>
    /// Ordered native hitbox-list identities from the frame components at
    /// $A2:CB87-$CCA9. Index follows the ten-byte physical-frame stride.
    /// </summary>
    private static readonly ushort[] ListPointers =
    [
        0xd034, 0xd04e, 0xd05c, 0xd076, 0xd090, 0xd0aa,
        0xd0c4, 0xd0de, 0xd0f8, 0xd11e, 0xd138, 0xd15e,
        0xd178, 0xd19e, 0xd1b8, 0xd1de, 0xd1f8, 0xd206,
        0xd220, 0xd23a, 0xd254, 0xd26e, 0xd288, 0xd2a2,
        0xd2c8, 0xd2e2, 0xd308, 0xd322, 0xd348, 0xd362,
    ];
    private static readonly ushort[] FrameKeys = BuildFrameKeys();
    private static readonly Dictionary<ushort, MaridiaLargeSnailCollisionHitbox[]> Lists = new()
    {
        [0xd034] = [new(-16, -17, -8, 16, SafeTouch, Shot), new(-8, -17, 14, 16, SafeTouch, NoShot)],
        [0xd04e] = [new(-16, -17, 14, 16, SafeTouch, NoShot)],
        [0xd05c] = [new(-1, -17, 14, 16, SafeTouch, NoShot), new(-17, -17, 0, 16, SafeTouch, Shot)],
        [0xd076] = [new(-20, -8, 0, 8, DamageTouch, Shot), new(0, -17, 13, 16, DamageTouch, NoShot)],
        [0xd090] = [new(-22, -8, 0, 7, DamageTouch, Shot), new(0, -17, 14, 16, DamageTouch, NoShot)],
        [0xd0aa] = [new(-25, -9, 0, 8, DamageTouch, Shot), new(0, -18, 14, 16, DamageTouch, NoShot)],
        [0xd0c4] = [new(-24, -8, 0, 9, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
        [0xd0de] = [new(-27, -8, 0, 8, DamageTouch, Shot), new(0, -18, 15, 16, DamageTouch, NoShot)],
        [0xd0f8] = [new(-16, 0, 0, 16, SafeTouch, Shot), new(-16, -16, 0, 0, SafeTouch, NoShot), new(0, -16, 14, 16, SafeTouch, NoShot)],
        [0xd11e] = [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
        [0xd138] = [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, NoShot), new(0, 0, 15, 16, SafeTouch, Shot)],
        [0xd15e] = [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 16, SafeTouch, Shot)],
        [0xd178] = [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 15, 0, SafeTouch, Shot), new(0, 0, 15, 16, SafeTouch, NoShot)],
        [0xd19e] = [new(-16, -18, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
        [0xd1b8] = [new(-16, 0, 0, 16, SafeTouch, NoShot), new(-16, -17, 0, 0, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [0xd1de] = [new(-16, -17, 8, 16, SafeTouch, NoShot), new(8, -17, 16, 16, SafeTouch, Shot)],
        [0xd1f8] = [new(-16, -17, 16, 16, SafeTouch, NoShot)],
        [0xd206] = [new(-16, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 16, SafeTouch, Shot)],
        [0xd220] = [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 20, 8, DamageTouch, Shot)],
        [0xd23a] = [new(-16, -17, -1, 16, DamageTouch, NoShot), new(0, -8, 22, 8, DamageTouch, Shot)],
        [0xd254] = [new(-16, -18, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
        [0xd26e] = [new(-15, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 24, 8, DamageTouch, Shot)],
        [0xd288] = [new(-16, -17, 0, 16, DamageTouch, NoShot), new(0, -8, 25, 8, DamageTouch, Shot)],
        [0xd2a2] = [new(-15, -16, 0, 16, SafeTouch, NoShot), new(0, -16, 16, 0, SafeTouch, NoShot), new(0, 0, 16, 16, SafeTouch, Shot)],
        [0xd2c8] = [new(-15, -17, 15, 0, SafeTouch, NoShot), new(-15, 0, 15, 16, SafeTouch, Shot)],
        [0xd2e2] = [new(-16, 0, 0, 17, SafeTouch, Shot), new(-16, -17, 0, 0, SafeTouch, NoShot), new(0, -17, 14, 17, SafeTouch, NoShot)],
        [0xd308] = [new(-16, -17, 0, 16, SafeTouch, Shot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [0xd322] = [new(-16, -17, 0, 0, SafeTouch, Shot), new(-16, 0, 0, 16, SafeTouch, NoShot), new(0, -17, 14, 16, SafeTouch, NoShot)],
        [0xd348] = [new(-16, -17, 15, 0, SafeTouch, Shot), new(-16, 0, 15, 16, SafeTouch, NoShot)],
        [0xd362] = [new(-15, -17, 0, 16, SafeTouch, NoShot), new(0, -17, 16, 0, SafeTouch, Shot), new(0, 0, 16, 16, SafeTouch, NoShot)],
    };

    internal static ReadOnlySpan<ushort> FramePointers => FrameKeys;
    internal static IEnumerable<ushort> HitboxPointers => Lists.Keys;

    internal static bool HasFrame(ushort frame) =>
        frame >= FirstFrame && frame <= LastFrame &&
        (frame - FirstFrame) % FrameStride == 0;

    internal static ushort HitboxListAt(ushort frame) =>
        HasFrame(frame) ? ListPointers[(frame - FirstFrame) / FrameStride] :
            throw new InvalidDataException(
                $"Oum frame $A2:{frame:X4} has no compiled collision.");

    internal static ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> HitboxesAt(ushort list) =>
        Lists.TryGetValue(list, out MaridiaLargeSnailCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Oum hitbox list $A2:{list:X4} is not compiled.");

    private static ushort[] BuildFrameKeys()
    {
        var keys = new ushort[ListPointers.Length];
        for (int index = 0; index < keys.Length; index++)
            keys[index] = unchecked((ushort)(FirstFrame + index * FrameStride));
        return keys;
    }
}
