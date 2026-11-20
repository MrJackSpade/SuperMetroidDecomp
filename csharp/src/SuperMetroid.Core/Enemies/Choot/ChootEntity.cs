namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChootEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => ChootBehavior.InitializeChoot(Occupant, Occupant.SpawnSettings<ChootSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ChootBehavior.RunChootMainAi(Occupant, Occupant.SpawnSettings<ChootSpawnSettings>(), Place, Place.RequireSpeciesState<ChootEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ChootBehavior.AdvanceChootTypedAnimation(Place, Place.RequireSpeciesState<ChootEnemyState>());
}
