namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with frozen-state behavior.</summary>
internal interface IEnemyFrozenAi
{
    /// <summary>
    /// Runs the frozen enemy's own step before the common thaw (a Rinka's ending); true when it ended the enemy's turn.
    /// </summary>
    bool PreemptFrozenTurn();

    /// <summary>Runs the enemy's frozen behaviour after the common thaw step.</summary>
    void RunFrozen();
}
