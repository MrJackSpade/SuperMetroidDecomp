namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy that may react to a normal bomb in its own way: before the check for the ignoring reaction, in place of
/// common bomb damage, or around it.
/// </summary>
internal interface IEnemyBombReaction : IEnemyEntity
{
    /// <summary>
    /// Runs the enemy's own reaction to the bomb, which comes before the check for the ignoring reaction, and returns
    /// true; returns false without running anything when it has none for this contact.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    bool TryReactToBomb(EnemyBombContext context);

    /// <summary>
    /// Runs the enemy's own reaction to a bomb whose selected reaction does not ignore it, in place of common bomb damage,
    /// and returns true; returns false without running anything when it has none for this contact.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    bool TryReplaceCommonBombDamage(EnemyBombContext context);

    /// <summary>The stages the enemy runs around common bomb damage for this contact, or null for the plain common damage.</summary>
    /// <param name="context">The bomb contact.</param>
    EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context);
}
