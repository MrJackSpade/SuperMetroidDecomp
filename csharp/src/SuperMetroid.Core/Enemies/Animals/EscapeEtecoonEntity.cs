namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeEtecoonEntity(EscapeAnimalsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEscapeEtecoon(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEscapeEtecoonMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EscapeAnimalsBehavior.AdvanceEscapeEtecoonTypedAnimation(Place, context);
}
