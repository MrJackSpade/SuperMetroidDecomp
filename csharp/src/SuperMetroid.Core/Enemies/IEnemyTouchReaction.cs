namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus touching it.</summary>
internal interface IEnemyTouchReaction : IEnemyEntity
{
    /// <summary>Whether the enemy reacts to Samus's touch now; the touch pass skips one that does not.</summary>
    bool ReactsToTouch { get; }

    /// <summary>Runs the enemy's reaction to Samus touching it.</summary>
    /// <param name="context">The touch.</param>
    void ReactToTouch(EnemyTouchContext context);
}
