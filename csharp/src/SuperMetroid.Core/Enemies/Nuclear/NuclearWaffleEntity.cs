namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One Nuclear Waffle occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Nuclear Waffle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NuclearWaffleEntity(NuclearWaffleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNuclearWaffle(Occupant, Occupant.SpawnSettings<NuclearWaffleSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNuclearWaffleMainAi(Occupant, Occupant.SpawnSettings<NuclearWaffleSpawnSettings>(), Place, Place.RequireSpeciesState<NuclearWaffleEnemyState>());

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NuclearWaffleBehavior.AdvanceNuclearWaffleTypedAnimation(Place, Place.RequireSpeciesState<NuclearWaffleEnemyState>());
}
