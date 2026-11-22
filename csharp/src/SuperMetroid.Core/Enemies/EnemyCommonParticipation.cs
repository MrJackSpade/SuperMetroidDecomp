using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The responses the engine shares across species, which a definition names (#1759): the ordinary touch and shot, the
/// reaction that does nothing, and the inert hurt. An entity answers from them for every contact its definition names a
/// shared response for; a species' own reactions are its entity's, which it checks before these.
/// </summary>
[RoomLifetime]
internal sealed partial class EnemyCommonParticipation
{
    /// <summary>The enemy system's collision, resolved at use because the system builds it after its species.</summary>
    private readonly Func<EnemyCollision> _collision;

    /// <summary>
    /// Creates the participation each room's enemy system shares with the entities it builds. It takes the collision in an
    /// ordinary constructor rather than a primary one, so a restore of the game's state makes the participation before it
    /// reads the enemy system the collision resolves through: that system holds the participation, and a state can reach
    /// the participation first, through an entity a species' part offer holds (#2554).
    /// </summary>
    /// <param name="collision">The enemy system's collision, resolved at use because the system builds it after its species.</param>
    internal EnemyCommonParticipation(Func<EnemyCollision> collision) => _collision = Ensure.NotNull(collision);

    /// <summary>
    /// The shot reaction of the ordinary and the ignoring shot: the common shot reaction. An ignoring hitbox or
    /// definition stops the shot, and nothing else happens.
    /// </summary>
    internal static EnemyShotReaction CommonShot { get; } = new();

    /// <summary>The Power Bomb reaction that ignores the blast: it replaces common damage with nothing.</summary>
    internal static EnemyPowerBombReaction IgnoringPowerBomb { get; } = new() { ReplacesCommon = IgnorePowerBomb };

    /// <summary>Does nothing in place of a Power Bomb's common damage: the enemy ignores the blast.</summary>
    private static void IgnorePowerBomb(EnemyPowerBombContext context)
    {
    }

    /// <summary>The common Power Bomb reaction: the blast's common damage, with no stage of a species' own.</summary>
    internal static EnemyPowerBombReaction CommonPowerBomb { get; } = new();

    /// <summary>Whether <paramref name="definition"/> names a shared touch reaction: the ordinary one or the ignoring one.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static bool ReactsToTouch(RoomEnemyDefinition definition) =>
        definition.Touch == EnemyReaction.Ignore || definition.Touch == EnemyReaction.OrdinaryTouch;

    /// <summary>
    /// Runs the shared touch reaction the touched enemy's definition names: the ordinary touch damages Samus or the enemy,
    /// and the ignoring touch reports the collision and does nothing else.
    /// </summary>
    /// <param name="context">The touch.</param>
    internal void ReactToTouch(EnemyTouchContext context)
    {
        if (context.Occupant.Definition.Touch == EnemyReaction.OrdinaryTouch)
            _collision().ResolveOrdinaryTouch(context);
    }

    /// <summary>
    /// The shared shot reaction <paramref name="definition"/> names, or the reaction of an enemy shots pass when it names
    /// a species' own that its entity does not answer.
    /// </summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static EnemyShotReaction ShotReaction(RoomEnemyDefinition definition) =>
        definition.Shot == EnemyReaction.Ignore || definition.Shot == EnemyReaction.OrdinaryShot ? CommonShot : EnemyShotReaction.ShotsPass;

    /// <summary>
    /// The shared Power Bomb reaction <paramref name="definition"/> names: the ignoring one, or the common Power Bomb
    /// damage.
    /// </summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static EnemyPowerBombReaction PowerBombReaction(RoomEnemyDefinition definition) =>
        definition.PowerBombReaction == EnemyReaction.Ignore ? IgnoringPowerBomb : CommonPowerBomb;

    /// <summary>Whether <paramref name="definition"/> names the inert hurt response, which runs in place of main AI and does nothing.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static bool HasInertHurt(RoomEnemyDefinition definition) =>
        definition.HurtResponse == EnemyHurtResponse.Inert;
}
