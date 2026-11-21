namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Torizo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CorpseEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <summary>The species' DeadTorizoShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadTorizoShotReaction => field ??= DeadTorizoBehavior.DeadTorizoShotReaction(State);

    /// <summary>The species' DeadTorizoPowerBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnDeadTorizoPowerBombReaction => field ??= DeadTorizoBehavior.DeadTorizoPowerBombReaction(State);

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTorizoBehavior.ShownDeadTorizoRows(State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DeadTorizoEnemyState State { get; } = species.InitializeDeadTorizo(place);

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
