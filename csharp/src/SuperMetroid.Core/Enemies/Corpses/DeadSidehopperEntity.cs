namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CorpseEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(Place);

    /// <inheritdoc/>
    public void Initialize() => species.InitializeDeadSidehopper(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadSidehopperMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadSidehopperBehavior.AdvanceDeadSidehopperTypedAnimation(Place);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => species.ReactToDeadSidehopperTouch(context);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => DeadSidehopperBehavior.DeadSidehopperShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => species.DeadSidehopperPowerBombReaction;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadSidehopperBehavior.ReplaceDeadSidehopperBombDamage(context);
}
