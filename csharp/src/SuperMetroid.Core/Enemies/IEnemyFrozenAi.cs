namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with frozen-state behavior.</summary>
internal interface IEnemyFrozenAi : IEnemyEntity
{
    /// <summary>The frozen preemption (Rinka's termination), or null; when it returns true the turn ends.</summary>
    EnemyFrozenPreemption? FindFrozenPreemption();

    /// <summary>Every frozen handler the enemy runs, in order.</summary>
    IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers();
}
