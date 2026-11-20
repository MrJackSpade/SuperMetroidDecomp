namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One Fune occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fune/Namihe species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FuneEntity(FuneNamiheBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FuneNamiheBehavior.InitializeFune(Occupant, Occupant.SpawnSettings<FuneNamiheSpawnSettings>(), Occupant.SpawnSettings<FuneSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FuneNamiheBehavior.RunFuneMain(Occupant, Occupant.SpawnSettings<FuneNamiheSpawnSettings>(), Occupant.SpawnSettings<FuneSpawnSettings>(), Place, Place.RequireSpeciesState<FuneEnemyState>());

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFuneNamiheTypedAnimation<FuneEnemyState>(Occupant, Occupant.SpawnSettings<FuneNamiheSpawnSettings>(), Place, Place.RequireSpeciesState<FuneEnemyState>());
}
