namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose private shot callbacks the normal-bomb collision path dispatches like the common one.</summary>
internal interface IEnemyPrivateShotCallback : IEnemyEntity
{
    /// <summary>Whether <paramref name="callback"/>, selected from the header or a hitbox, is one of the enemy's translated private shot callbacks.</summary>
    /// <param name="callback">The named callback the header or hitbox selects, or null.</param>
    bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback);
}
