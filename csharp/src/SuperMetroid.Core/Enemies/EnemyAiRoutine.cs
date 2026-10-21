namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy header's AI entry runs.</summary>
public enum EnemyAiRoutine
{
    /// <summary>The species' own routine.</summary>
    SpeciesRoutine,

    /// <summary>The shared routine that does nothing.</summary>
    CommonNoOp,
}
