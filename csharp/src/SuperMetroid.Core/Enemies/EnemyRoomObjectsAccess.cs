using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>Reaches the enemy PLM families from a step context's room PLM owner.</summary>
internal static class EnemyRoomObjectsAccess
{
    /// <summary>The enemy PLM families <paramref name="roomObjects"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no enemy PLM families.</exception>
    internal static IEnemyRoomObjects Enemies(this IRoomObjects roomObjects) =>
        roomObjects as IEnemyRoomObjects ?? throw new InvalidOperationException($"{roomObjects.GetType().Name} runs no enemy PLM families.");
}
