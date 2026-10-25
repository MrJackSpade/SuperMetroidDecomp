namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The movement routine a Stoke projectile runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum StokeProjectileFunction : ushort
{
    /// <summary>$86:DB62.</summary>
    MoveLeft,

    /// <summary>$86:DB8C.</summary>
    MoveRight,
}
