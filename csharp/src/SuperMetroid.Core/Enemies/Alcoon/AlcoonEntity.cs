namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One Alcoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Alcoon species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
internal sealed class AlcoonEntity(AlcoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal AlcoonEnemyState State { get; } = species.SetUpAlcoon(place, setup.SurroundingsOf(place));

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunAlcoonMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceAlcoonTypedAnimation(Place, State, context);
}
