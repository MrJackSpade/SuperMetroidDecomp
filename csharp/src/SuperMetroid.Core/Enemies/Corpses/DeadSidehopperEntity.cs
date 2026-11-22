namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, DeadSidehopperEnemyState state)
    : CorpseEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The dead Sidehopper species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        DeadSidehopperSpawnSettings settings)
        : this(species, occupant, place, common, species.InitializeDeadSidehopper(settings, place))
    {
    }

    /// <summary>The species' DeadSidehopperShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadSidehopperShotReaction => field ??= DeadSidehopperBehavior.DeadSidehopperShotReaction(MarkShotWithoutDamage);

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DeadSidehopperEnemyState State { get; } = state;

    /// <summary>Runs the corpse's own Power Bomb reaction, in place of its common damage.</summary>
    /// <param name="context">The blast meeting the corpse.</param>
    internal void ReactToPowerBomb(EnemyPowerBombContext context) => species.ResolveDeadSidehopperPowerBomb(context, State);

    /// <summary>Marks a shot's hit on the corpse, which ignores it until drained and then starts rotting.</summary>
    /// <param name="c">The shot meeting the corpse.</param>
    /// <returns>Always true: the corpse takes no damage or impact.</returns>
    internal bool MarkShotWithoutDamage(in EnemyShotContext c)
    {
        c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        DeadSidehopperBehavior.ResolveDeadSidehopperShot(c.Enemy, State);
        return true;
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadSidehopperMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadSidehopperBehavior.AdvanceDeadSidehopperTypedAnimation(Place, State);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => species.ReactToDeadSidehopperTouch(context, State);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => OwnDeadSidehopperShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => OwnPowerBomb;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadSidehopperBehavior.ReplaceDeadSidehopperBombDamage(context, State);

    /// <summary>The corpse's own Power Bomb reaction, built once for this corpse with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= DeadSidehopperBehavior.DeadSidehopperPowerBombReaction(ReactToPowerBomb);
}
