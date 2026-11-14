namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="slot">The slot the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeEtecoonEntity(EscapeAnimalsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEscapeEtecoon(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEscapeEtecoonMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEscapeAnimalTypedAnimation(Slot, context);
}
