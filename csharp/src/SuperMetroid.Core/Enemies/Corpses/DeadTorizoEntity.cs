namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Torizo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Torizo species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CorpseEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTorizoBehavior.ShownDeadTorizoRows(Place);

    /// <inheritdoc/>
    public void Initialize() => species.InitializeDeadTorizo(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTorizoMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTorizoBehavior.AdvanceDeadTorizoTypedAnimation(Place);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => DeadTorizoBehavior.ReactToDeadTorizoTouch(context);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => species.DeadTorizoShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => species.DeadTorizoPowerBombReaction;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadTorizoBehavior.ReplaceDeadTorizoBombDamage(context);
}
