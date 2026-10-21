using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>Reaches the enemy PLM families from a step context's room PLM owner.</summary>
internal static class EnemyRoomPlmsAccess
{
    /// <summary>The enemy PLM families <paramref name="plms"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no enemy PLM families.</exception>
    internal static IEnemyRoomPlms Enemies(this IRoomPlms plms) =>
        plms as IEnemyRoomPlms ?? throw new InvalidOperationException($"{plms.GetType().Name} runs no enemy PLM families.");
}
