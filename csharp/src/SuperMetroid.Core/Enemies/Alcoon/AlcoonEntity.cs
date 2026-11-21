namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One Alcoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Alcoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
internal sealed class AlcoonEntity(AlcoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly AlcoonEnemyState state = species.InitializeAlcoon(place, surroundings.Level);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunAlcoonMainAi(Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceAlcoonTypedAnimation(Occupant, Place, state, context);
}
