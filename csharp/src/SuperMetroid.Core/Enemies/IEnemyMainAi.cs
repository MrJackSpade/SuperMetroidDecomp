namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with a main AI, run when no AI-state handler is selected.</summary>
internal interface IEnemyMainAi : IEnemyEntity
{
    /// <summary>The main-AI handler, or null when the header names none this build translates.</summary>
    EnemyMainAiHandler? FindMain();
}
