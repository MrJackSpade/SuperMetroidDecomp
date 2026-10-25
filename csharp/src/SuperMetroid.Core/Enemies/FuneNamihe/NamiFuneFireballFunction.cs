namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The movement routine a Fune/Namihe fireball runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum NamiFuneFireballFunction : ushort
{
    /// <summary>$86:DF40.</summary>
    MovingLeft = 1,

    /// <summary>$86:DF6A.</summary>
    MovingRight,
}
