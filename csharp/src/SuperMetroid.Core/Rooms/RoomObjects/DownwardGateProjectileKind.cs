namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which gate projectile a downward gate room object asks the enemy projectiles to spawn (#1395).</summary>
public enum DownwardGateProjectileKind
{
    /// <summary>The gate that starts closed.</summary>
    Closed,

    /// <summary>The gate that moves when shot.</summary>
    Moving,
}
