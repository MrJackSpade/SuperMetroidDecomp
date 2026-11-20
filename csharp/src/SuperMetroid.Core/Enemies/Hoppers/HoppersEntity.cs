namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hoppers occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Hoppers species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoppersEntity(HoppersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeHopper(Place, Occupant, Occupant.SpawnSettings<HopperSpawnSettings>());

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoppersMainAi(Occupant, Occupant.SpawnSettings<HopperSpawnSettings>(), Place, Place.RequireSpeciesState<HopperEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceHoppersTypedAnimation(Place, Place.RequireSpeciesState<HopperEnemyState>());
}
