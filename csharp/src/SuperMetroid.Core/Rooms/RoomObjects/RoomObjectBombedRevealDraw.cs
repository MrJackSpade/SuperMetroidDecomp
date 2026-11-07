namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The three bombed-reveal draw lists.</summary>
internal enum RoomObjectBombedRevealDraw
{
    /// <summary>A bombed respawning one-block crumble block; its intact parent is a crumble block.</summary>
    CrumbleSingle,
    /// <summary>A bombed Power Bomb block; its intact parent is a shot block.</summary>
    PowerBomb,
    /// <summary>A bombed Super Missile block; its intact parent is a shot block.</summary>
    SuperMissile,
}
