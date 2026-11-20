namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Tourian corpse occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Tourian corpses' species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CorpseEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTourianBehavior.ShownDeadTourianRows(Place);

    /// <inheritdoc/>
    public void Initialize() => DeadTourianBehavior.InitializeDeadTourianCorpse(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTourianMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation(Place);

    /// <inheritdoc/>
    private protected override void ReactToOwnTouch(EnemyTouchContext context) => DeadTourianBehavior.ReactToDeadTourianTouch(context);

    /// <inheritdoc/>
    private protected override EnemyShotReaction OwnShotReaction => DeadTourianBehavior.DeadTourianShotReaction;

    /// <inheritdoc/>
    private protected override EnemyPowerBombReaction OwnPowerBombReaction => DeadTourianBehavior.DeadTourianPowerBombReaction;

    /// <inheritdoc/>
    private protected override void ReplaceBombDamage(EnemyBombContext context) => DeadTourianBehavior.ReplaceDeadTourianBombDamage(context);
}
