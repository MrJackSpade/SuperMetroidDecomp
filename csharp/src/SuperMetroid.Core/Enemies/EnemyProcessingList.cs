namespace SuperMetroid.Core.Enemies;

/// <summary>Whether the enemy processing list was prepared before the main phase.</summary>
internal enum EnemyProcessingList
{
    /// <summary>The main phase prepares it.</summary>
    Unprepared,

    /// <summary>An earlier phase prepared it.</summary>
    Prepared,
}
