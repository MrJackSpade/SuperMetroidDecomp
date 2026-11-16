namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Lower Norfair Rio occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Lower Norfair Rio species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class LowerNorfairRioEntity(LowerNorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeLowerNorfairRio(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunLowerNorfairRioMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => LowerNorfairRioBehavior.AdvanceLowerNorfairRioTypedAnimation(Place, context);
}
