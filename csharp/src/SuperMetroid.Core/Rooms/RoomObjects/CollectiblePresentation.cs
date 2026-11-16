namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The three ways an item is placed in a room: in the open, in a Chozo orb or hidden in a shot block.</summary>
public enum CollectiblePresentation
{
    /// <summary>The item begins visible and can be acquired by Samus contact.</summary>
    Exposed,
    /// <summary>The item begins inside a projectile-triggered Chozo orb; breaking the shell does not persist between room visits.</summary>
    ChozoOrb,
    /// <summary>The item begins in a concealed shot block that temporarily reveals the item and later restores its saved block.</summary>
    ShotBlock,
}
