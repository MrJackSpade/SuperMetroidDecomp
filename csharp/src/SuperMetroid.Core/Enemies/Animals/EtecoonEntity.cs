namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Etecoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Etecoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EtecoonEntity(EtecoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => EtecoonBehavior.InitializeEtecoon(Occupant, Occupant.SpawnSettings<EtecoonSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEtecoonMainAi(Place, Place.RequireSpeciesState<EtecoonEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EtecoonBehavior.AdvanceEtecoonTypedAnimation(Place, Place.RequireSpeciesState<EtecoonEnemyState>());
}
