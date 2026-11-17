namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Dachora occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="place">The place the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeDachoraEntity(EscapeAnimalsBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEscapeDachora(Place);

    /// <inheritdoc/>
    /// <remarks>The escape Dachora has no main behaviour; it moves through its animation.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEscapeDachoraTypedAnimation(Place, context);
}
