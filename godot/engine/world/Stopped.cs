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

    /// <summary>The player's own shots.</summary>
    PlayerShots = 4,

    /// <summary>Room objects, terrain and liquid.</summary>
    Room = 8,

    /// <summary>
    /// What the enemies launch and leave: their shots, what a death or a shot leaves, and pickups. While the enemies
    /// themselves are stopped and these are not, they go on moving and counting but strike no one.
    /// </summary>
    Launched = 16,

    /// <summary>Every projectile and pickup: the player's shots and what the enemies launch and leave.</summary>
    Projectiles = PlayerShots | Launched,

    /// <summary>Everything.</summary>
    Everything = Player | Enemies | Projectiles | Room,
}
