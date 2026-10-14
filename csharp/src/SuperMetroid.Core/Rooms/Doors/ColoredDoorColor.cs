namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The three projectile-gated door families in header order.</summary>
public enum ColoredDoorColor : byte
{
    /// <summary>Yellow cap, which accepts Power Bomb hits.</summary>
    Yellow,
    /// <summary>Green cap, which accepts Super Missile hits.</summary>
    Green,
    /// <summary>Red cap, which accepts Missile or Super Missile hits; a Super Missile immediately satisfies its hit threshold.</summary>
    Red,
}
