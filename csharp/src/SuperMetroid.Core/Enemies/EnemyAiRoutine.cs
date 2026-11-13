namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy definition's AI entry runs.</summary>
public enum EnemyAiRoutine
{
    /// <summary>The species' own behaviour.</summary>
    SpeciesRoutine,

    /// <summary>Nothing: the enemy does nothing at this point.</summary>
    Inert,
}
