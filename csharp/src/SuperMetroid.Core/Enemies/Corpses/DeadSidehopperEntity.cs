namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, DeadSidehopperSpawnSettings settings)
    : CorpseEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <summary>The species' DeadSidehopperShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadSidehopperShotReaction => field ??= DeadSidehopperBehavior.DeadSidehopperShotReaction(State);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal DeadSidehopperSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DeadSidehopperEnemyState State { get; } = species.InitializeDeadSidehopper(settings, place);

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
    private EnemyPowerBombReaction OwnPowerBomb => field ??= species.DeadSidehopperPowerBombReaction(State);
}
