using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The game-state services enemy AI calls during a room: the union of every child operation's
/// service contract plus the RNG, events, defeated bosses and room-object presence the AI uses directly.
/// </summary>
public interface IRoomEnemyServices : IChozoStatueRoomServices, IMotherBrainRoomServices
{
    /// <summary>The game RNG.</summary>
    IEnemyRandomNumbers Random { get; }

    /// <summary>The story events that have happened.</summary>
    IStoryEvents Events { get; }

    /// <summary>How many Zebetites have been destroyed.</summary>
    IZebetiteProgress ZebetiteProgress { get; }

    /// <summary>The current area's defeated bosses.</summary>
    IDefeatedBossesRecorder Bosses { get; }

    /// <summary>The active room-object population.</summary>
    IRoomObjectPresence RoomObjects { get; }
}
