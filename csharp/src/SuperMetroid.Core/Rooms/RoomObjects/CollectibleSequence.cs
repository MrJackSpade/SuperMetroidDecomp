namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The short animations a collectible plays at its block (<see cref="CollectibleSequenceAnimations"/>, #1994).</summary>
internal enum CollectibleSequence
{
    /// <summary>A shot Chozo orb bursting open around its item.</summary>
    OrbBurst,
    /// <summary>A shot block crumbling away from the item it conceals.</summary>
    ShotBlockReveal,
    /// <summary>A shot block forming again over an item nobody took.</summary>
    ShotBlockConceal,
    /// <summary>A collected item's shot block growing back.</summary>
    ShotBlockRespawn,
}
