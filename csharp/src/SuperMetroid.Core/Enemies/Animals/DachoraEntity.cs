namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>A Dachora's body: its run, charge and shinespark, with the shared reactions.</summary>
/// <param name="species">The Dachora species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DachoraEntity(DachoraBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, DachoraSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal DachoraEnemyState State { get; } = species.InitializeDachora(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDachoraMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DachoraBehavior.AdvanceDachoraTypedAnimation(Place, State, context);
}
