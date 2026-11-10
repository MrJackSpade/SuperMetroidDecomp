using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec between <see cref="SamusProjectileDirection"/> and the cartridge's ten-way aim index, which numbers the
/// aim directions clockwise from up (facing right) through down to up (facing left). The cartridge's direction-indexed
/// tables (projectile headers, muzzle and flare origins, trail directions, arm-cannon covers) and a projectile's
/// direction word, whose low nibble holds it, are read by it; the flare origin tables continue past the ten directions
/// with the standing beam flare's turning placements. This is the one place a direction becomes its index or an index
/// its direction.
/// </summary>
[EncodingOwner("projectile aim index")]
internal static class SamusProjectileDirectionCodec
{
    /// <summary>The number of directions the aim index names.</summary>
    internal const int Count = 10;

    /// <summary>The directions in aim-index order.</summary>
    internal static IReadOnlyList<SamusProjectileDirection> InAimOrder { get; } =
    [
        SamusProjectileDirection.UpFacingRight, SamusProjectileDirection.UpRight, SamusProjectileDirection.Right,
        SamusProjectileDirection.DownRight, SamusProjectileDirection.DownFacingRight, SamusProjectileDirection.DownFacingLeft,
        SamusProjectileDirection.DownLeft, SamusProjectileDirection.Left, SamusProjectileDirection.UpLeft,
        SamusProjectileDirection.UpFacingLeft,
    ];

    /// <summary>The turning placements in the order the flare origin tables keep them after the ten directions.</summary>
    internal static IReadOnlyList<SamusTurningAim> TurningPlacementOrder { get; } =
        [SamusTurningAim.Up, SamusTurningAim.Neutral, SamusTurningAim.Down];

    /// <summary>The direction's aim index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    internal static byte ToAimIndex(this SamusProjectileDirection direction) => checked((byte)PlaceIn(InAimOrder, direction));

    /// <summary>The direction an aim index names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index names no direction.</exception>
    internal static SamusProjectileDirection FromAimIndex(int index) =>
        index is >= 0 and < Count
            ? InAimOrder[index]
            : throw new ArgumentOutOfRangeException(nameof(index), index, "The aim index names ten directions.");

    /// <summary>
    /// The row of a flare origin table that holds the placement at <paramref name="aim"/>: a muzzle's aim index, or a
    /// turning placement's row after the ten directions.
    /// </summary>
    /// <exception cref="ArgumentException">The flare aim names neither a muzzle nor a turn.</exception>
    internal static int FlarePlacementRow(SamusFlareAim aim) =>
        aim.Muzzle is { } muzzle ? muzzle.ToAimIndex()
        : aim.Turning is { } turning ? Count + PlaceIn(TurningPlacementOrder, turning)
        : throw new ArgumentException("A flare is placed at a muzzle or while turning.", nameof(aim));

    /// <summary>Where <paramref name="value"/> stands in <paramref name="order"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The order does not hold the value.</exception>
    private static int PlaceIn<T>(IReadOnlyList<T> order, T value) where T : struct, Enum
    {
        for (int place = 0; place < order.Count; place++)
            if (EqualityComparer<T>.Default.Equals(order[place], value))
                return place;
        throw new ArgumentOutOfRangeException(nameof(value), value, $"Undefined {typeof(T).Name}.");
    }
}
