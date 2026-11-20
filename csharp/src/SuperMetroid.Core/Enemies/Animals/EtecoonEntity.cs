namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Etecoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Etecoon species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class EtecoonEntity(EtecoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EtecoonSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal EtecoonEnemyState State { get; } = EtecoonBehavior.InitializeEtecoon(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEtecoonMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EtecoonBehavior.AdvanceEtecoonTypedAnimation(Place, State, context);
}
