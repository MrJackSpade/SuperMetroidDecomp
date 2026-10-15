namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Table-order access to the ten projectile directions.</summary>
public static class SamusProjectileDirections
{
    /// <summary>The direction at one position of a ten-entry native direction table.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the ten directions.</exception>
    public static SamusProjectileDirection At(int tableIndex) => tableIndex switch
    {
        0 => SamusProjectileDirection.UpFacingRight,
        1 => SamusProjectileDirection.UpRight,
        2 => SamusProjectileDirection.Right,
        3 => SamusProjectileDirection.DownRight,
        4 => SamusProjectileDirection.DownFacingRight,
        5 => SamusProjectileDirection.DownFacingLeft,
        6 => SamusProjectileDirection.DownLeft,
        7 => SamusProjectileDirection.Left,
        8 => SamusProjectileDirection.UpLeft,
        9 => SamusProjectileDirection.UpFacingLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(tableIndex), tableIndex, "Projectile direction tables have ten entries."),
    };
}
