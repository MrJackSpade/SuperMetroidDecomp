namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>A Dachora's body: its run, charge and shinespark, with the shared reactions.</summary>
/// <param name="species">The Dachora species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DachoraEntity(DachoraBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeDachora(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDachoraMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DachoraBehavior.AdvanceDachoraTypedAnimation(Place, context);
}
