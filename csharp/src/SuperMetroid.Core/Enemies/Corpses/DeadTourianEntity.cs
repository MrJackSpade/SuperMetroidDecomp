namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Tourian corpse occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Tourian corpses' species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CorpseSpawnSettings settings)
    : CorpseEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <summary>The species' DeadTourianShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadTourianShotReaction => field ??= DeadTourianBehavior.DeadTourianShotReaction(state);

    /// <summary>The species' DeadTourianPowerBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnDeadTourianPowerBombReaction => field ??= DeadTourianBehavior.DeadTourianPowerBombReaction(state);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal CorpseSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTourianBehavior.ShownDeadTourianRows(state);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly DeadTourianCorpseEnemyState state = DeadTourianBehavior.InitializeDeadTourianCorpse(place, occupant, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTourianMainAi(Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation(Place, state);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => DeadTourianBehavior.ReactToDeadTourianTouch(context, state);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => OwnDeadTourianShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => OwnDeadTourianPowerBombReaction;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadTourianBehavior.ReplaceDeadTourianBombDamage(context, state);
}
