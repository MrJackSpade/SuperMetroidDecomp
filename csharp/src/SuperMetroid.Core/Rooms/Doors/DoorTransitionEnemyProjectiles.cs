namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Whether a door-transition frame runs enemy projectiles before drawing.</summary>
public enum DoorTransitionEnemyProjectiles
{
    /// <summary>Enemy projectiles are only drawn.</summary>
    Held,

    /// <summary>Enemy projectiles run, then draw.</summary>
    Run,
}
