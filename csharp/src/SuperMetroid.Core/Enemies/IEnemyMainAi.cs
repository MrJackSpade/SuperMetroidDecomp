namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with a main AI, run when no AI-state handler is selected.</summary>
internal interface IEnemyMainAi : IEnemyEntity
{
    /// <summary>The main-AI handler, or null when the definition names none this build models.</summary>
    EnemyMainAiHandler? FindMain();
}
