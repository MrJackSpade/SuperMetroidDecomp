using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses;

/// <summary>Reaches the boss room-object families from a step context's room-object owner.</summary>
internal static class BossRoomObjectsAccess
{
    /// <summary>The boss room-object families <paramref name="roomObjects"/> runs; every room's room-object owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no boss room-object families.</exception>
    internal static IBossRoomObjects Bosses(this IRoomObjects roomObjects) =>
        roomObjects as IBossRoomObjects ?? throw new InvalidOperationException($"{roomObjects.GetType().Name} runs no boss room-object families.");
}
