namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One Multiviola occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Multiviola species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MultiviolaEntity(MultiviolaBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => MultiviolaBehavior.InitializeMultiviola(Occupant, Occupant.SpawnSettings<MultiviolaSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMultiviolaMainAi(Occupant, Occupant.SpawnSettings<MultiviolaSpawnSettings>(), Place, Place.RequireSpeciesState<MultiviolaEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MultiviolaBehavior.AdvanceMultiviolaTypedAnimation(Place, Place.RequireSpeciesState<MultiviolaEnemyState>());
}
