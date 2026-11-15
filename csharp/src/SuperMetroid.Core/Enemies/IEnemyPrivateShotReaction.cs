namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with shot reactions of its own, which a normal bomb triggers as it does the common one.</summary>
internal interface IEnemyPrivateShotReaction : IEnemyEntity
{
    /// <summary>Whether <paramref name="reaction"/>, selected from the definition or a hitbox, is one of the enemy's private shot reactions.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    bool OwnsPrivateShotReaction(EnemyReaction? reaction);
}
