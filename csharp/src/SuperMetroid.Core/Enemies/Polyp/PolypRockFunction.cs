namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// The movement routine a Polyp rock runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum PolypRockFunction : ushort
{
    /// <summary>$86:BC16.</summary>
    Rising = 1,

    /// <summary>$86:BC8F.</summary>
    Falling,
}
