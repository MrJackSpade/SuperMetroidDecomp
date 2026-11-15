namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The responses the engine shares across species, which a definition names (#1759): the ordinary touch and shot, the
/// reaction that does nothing, and the inert hurt. An entity answers from them for every contact its definition names a
/// shared response for; a species' own reactions are its entity's, which it checks before these.
/// </summary>
/// <param name="collision">The enemy system's collision, resolved at use because the system builds it after its species.</param>
internal sealed class EnemyCommonParticipation(Func<EnemyCollision> collision)
{
    /// <summary>
    /// The shot reaction of the ordinary and the ignoring shot: common shot AI with no species stage. The shot pass ends an
    /// ignoring hitbox's or definition's shot after the collision prelude.
    /// </summary>
    internal static EnemyShotReaction CommonShot { get; } = new();

    /// <summary>The Power Bomb reaction that ignores the blast: it replaces common damage with nothing.</summary>
    internal static EnemyPowerBombReaction IgnoringPowerBomb { get; } = new() { ReplacesCommon = static _ => { } };

    /// <summary>Whether <paramref name="definition"/> names a shared touch reaction: the ordinary one or the ignoring one.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static bool ReactsToTouch(RoomEnemyDefinition definition) =>
        definition.Touch == EnemyReaction.Ignore || definition.Touch == EnemyReaction.NormalEnemyTouch;

    /// <summary>
    /// Runs the shared touch reaction the touched enemy's definition names: the ordinary touch damages Samus or the enemy,
    /// and the ignoring touch reports the collision and does nothing else.
    /// </summary>
    /// <param name="context">The touch.</param>
    internal void ReactToTouch(EnemyTouchContext context)
    {
        if (context.Slot.Definition.Touch == EnemyReaction.NormalEnemyTouch)
            collision().ResolveNormalEnemyTouch(context.Slot, context.Samus);
    }

    /// <summary>The shared shot reaction <paramref name="definition"/> names, or null when it names none.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static EnemyShotReaction? ShotReaction(RoomEnemyDefinition definition) =>
        definition.Shot == EnemyReaction.Ignore || definition.Shot == EnemyReaction.NormalEnemyShot ? CommonShot : null;

    /// <summary>
    /// The shared Power Bomb reaction <paramref name="definition"/> names: the ignoring one, or null for common Power
    /// Bomb damage.
    /// </summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static EnemyPowerBombReaction? PowerBombReaction(RoomEnemyDefinition definition) =>
        definition.PowerBombReaction == EnemyReaction.Ignore ? IgnoringPowerBomb : null;

    /// <summary>Whether <paramref name="definition"/> names the inert hurt response, which runs in place of main AI and does nothing.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static bool HasInertHurt(RoomEnemyDefinition definition) =>
        definition.HurtResponse == EnemyHurtResponse.Inert;
}
