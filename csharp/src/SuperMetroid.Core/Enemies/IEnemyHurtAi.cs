namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may replace its main AI with a hurt AI while hurt.</summary>
internal interface IEnemyHurtAi : IEnemyEntity
{
    /// <summary>The hurt handler the enemy uses now, or null to fall through to the next handler.</summary>
    EnemyHurtAiHandler? FindHurt();
}
