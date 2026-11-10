namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One value for each of the ten directions a shot is fired in: what a definition that differs by the shot's direction
/// holds, read by the direction itself.
/// </summary>
/// <typeparam name="T">The value each direction has.</typeparam>
/// <param name="UpFacingRight">The value of a shot fired straight up while facing right.</param>
/// <param name="UpRight">The value of a shot fired diagonally up and right.</param>
/// <param name="Right">The value of a shot fired right.</param>
/// <param name="DownRight">The value of a shot fired diagonally down and right.</param>
/// <param name="DownFacingRight">The value of a shot fired straight down while facing right.</param>
/// <param name="DownFacingLeft">The value of a shot fired straight down while facing left.</param>
/// <param name="DownLeft">The value of a shot fired diagonally down and left.</param>
/// <param name="Left">The value of a shot fired left.</param>
/// <param name="UpLeft">The value of a shot fired diagonally up and left.</param>
/// <param name="UpFacingLeft">The value of a shot fired straight up while facing left.</param>
internal sealed record SamusProjectileDirectionMap<T>(
    T UpFacingRight, T UpRight, T Right, T DownRight, T DownFacingRight,
    T DownFacingLeft, T DownLeft, T Left, T UpLeft, T UpFacingLeft)
{
    /// <summary>The same value in every direction.</summary>
    /// <param name="every">The value every direction has.</param>
    internal SamusProjectileDirectionMap(T every)
        : this(every, every, every, every, every, every, every, every, every, every)
    {
    }

    /// <summary>The value of a shot fired in <paramref name="direction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    internal T For(SamusProjectileDirection direction) => direction switch
    {
        SamusProjectileDirection.UpFacingRight => UpFacingRight,
        SamusProjectileDirection.UpRight => UpRight,
        SamusProjectileDirection.Right => Right,
        SamusProjectileDirection.DownRight => DownRight,
        SamusProjectileDirection.DownFacingRight => DownFacingRight,
        SamusProjectileDirection.DownFacingLeft => DownFacingLeft,
        SamusProjectileDirection.DownLeft => DownLeft,
        SamusProjectileDirection.Left => Left,
        SamusProjectileDirection.UpLeft => UpLeft,
        SamusProjectileDirection.UpFacingLeft => UpFacingLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined shot direction."),
    };

    /// <summary>Every direction's value.</summary>
    internal IEnumerable<T> Values => [UpFacingRight, UpRight, Right, DownRight, DownFacingRight, DownFacingLeft, DownLeft, Left, UpLeft, UpFacingLeft];
}
