namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with frozen-state behavior.</summary>
internal interface IEnemyFrozenAi : IEnemyEntity
{
    /// <summary>
    /// Runs the frozen enemy's own tail before the common thaw (Rinka's termination); true when it ended the enemy's turn.
    /// </summary>
    bool PreemptFrozenTurn();

    /// <summary>Runs the enemy's frozen behaviour after the common thaw step.</summary>
    void RunFrozen();
}
