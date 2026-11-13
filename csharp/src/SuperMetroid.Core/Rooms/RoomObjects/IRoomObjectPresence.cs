namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the room's enemies ask of its room objects.</summary>
public interface IRoomObjectPresence
{
    /// <summary>Whether the Bomb Torizo statue's hand still stands in the room; Bomb Torizo sleeps until it has crumbled away.</summary>
    bool BombTorizoHandRemains { get; }
}
