namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus's bombs: special, after the no-op check, or a common reaction.</summary>
internal interface IEnemyBombReaction : IEnemyEntity
{
    /// <summary>The special bomb handler for this contact, or null.</summary>
    /// <param name="context">The bomb contact.</param>
    Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context);

    /// <summary>The special bomb handler that runs after the no-op check, or null.</summary>
    /// <param name="context">The bomb contact.</param>
    Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context);

    /// <summary>The common bomb reaction for this contact, or null.</summary>
    /// <param name="context">The bomb contact.</param>
    EnemyBombReaction? FindBombCommon(EnemyBombContext context);
}
