using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses;

/// <summary>Reaches the boss PLM families from a step context's room PLM owner.</summary>
internal static class BossRoomObjectsAccess
{
    /// <summary>The boss PLM families <paramref name="roomObjects"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no boss PLM families.</exception>
    internal static IBossRoomObjects Bosses(this IRoomObjects roomObjects) =>
        roomObjects as IBossRoomObjects ?? throw new InvalidOperationException($"{roomObjects.GetType().Name} runs no boss PLM families.");
}
