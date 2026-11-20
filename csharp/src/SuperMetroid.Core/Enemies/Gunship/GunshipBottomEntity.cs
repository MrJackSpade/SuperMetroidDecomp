namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's bottom hull with its entrance pad (#2146): set up beside the top hull, then moved and animated with the
/// ship, with the shared reactions.
/// </summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the bottom hull occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipBottomEntity(GunshipBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemySurroundingsInitialization, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => species.InitializeGunshipBottom(Occupant, Occupant.SpawnSettings<GunshipSpawnSettings>(), Place, surroundings.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, Place.RequireSpeciesState<GunshipPieceState>());
}
