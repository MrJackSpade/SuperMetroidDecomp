namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Dachora occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="slot">The slot the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeDachoraEntity(EscapeAnimalsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEscapeDachora(Slot);

    /// <inheritdoc/>
    /// <remarks>The escape Dachora has no main behaviour; it moves through its animation.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEscapeAnimalTypedAnimation(Slot, context);
}
