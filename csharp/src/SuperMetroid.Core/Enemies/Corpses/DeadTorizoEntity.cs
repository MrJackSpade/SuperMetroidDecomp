namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Torizo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, DeadTorizoEnemyState state)
    : CorpseEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The dead Torizo species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, species.InitializeDeadTorizo(place))
    {
    }

    /// <summary>The species' DeadTorizoShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadTorizoShotReaction => field ??= DeadTorizoBehavior.DeadTorizoShotReaction(MarkShotAndRot);

    /// <summary>The species' DeadTorizoPowerBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnDeadTorizoPowerBombReaction => field ??= DeadTorizoBehavior.DeadTorizoPowerBombReaction(StartRot);

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTorizoBehavior.ShownDeadTorizoRows(State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DeadTorizoEnemyState State { get; } = state;

    /// <summary>Starts the corpse rotting on a Power Bomb, in place of its common damage.</summary>
    /// <param name="context">The blast meeting the corpse.</param>
    internal void StartRot(EnemyPowerBombContext context) => DeadTorizoBehavior.TriggerDeadTorizoPowerBomb(context.Enemy, State);

    /// <summary>Marks a shot's hit on the corpse and starts it rotting, with no damage or explosion.</summary>
    /// <param name="c">The shot meeting the corpse.</param>
    /// <returns>Always true: the rot replaces the rest of the shot reaction.</returns>
    internal bool MarkShotAndRot(in EnemyShotContext c)
    {
        c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        DeadTorizoBehavior.TriggerDeadTorizoRotting(c.Enemy, State);
        return true;
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTorizoMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTorizoBehavior.AdvanceDeadTorizoTypedAnimation(Place, State);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => DeadTorizoBehavior.ReactToDeadTorizoTouch(context, State);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => OwnDeadTorizoShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => OwnDeadTorizoPowerBombReaction;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadTorizoBehavior.ReplaceDeadTorizoBombDamage(context, State);
}
