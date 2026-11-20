namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Etecoon occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="place">The place the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeEtecoonEntity(EscapeAnimalsBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal EscapeEtecoonEnemyState State { get; } = species.InitializeEscapeEtecoon(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEscapeEtecoonMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EscapeAnimalsBehavior.AdvanceEscapeEtecoonTypedAnimation(Place, context);
}
