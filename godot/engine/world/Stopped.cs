namespace Engine.World;

/// <summary>The groups of things in a room that can be stopped for an update while the rest run.</summary>
[Flags]
public enum Stopped
{
    /// <summary>Nothing is stopped.</summary>
    Nothing = 0,

    /// <summary>The player.</summary>
    Player = 1,

    /// <summary>Enemies.</summary>
    Enemies = 2,

    /// <summary>Projectiles and pickups.</summary>
    Projectiles = 4,

    /// <summary>Room objects, terrain and liquid.</summary>
    Room = 8,

    /// <summary>Everything.</summary>
    Everything = Player | Enemies | Projectiles | Room,
}
