namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with an initialization AI, run once after its identity is installed.</summary>
internal interface IEnemyInitialization : IEnemyEntity
{
    /// <summary>The initialization handler, or null when the header names none this build translates.</summary>
    EnemyInitializationAiHandler? FindInitialization();
}
