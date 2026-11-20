namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Geega (#2146), the yellow Pipe Bug: it waits, then flies straight and arcs away.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YellowPipeBugEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => PipeBugsBehavior.InitializeYellowPipeBug(Occupant, Occupant.SpawnSettings<YellowPipeBugSpawnSettings>(), Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => PipeBugsBehavior.RunYellowPipeBugMainAi(Occupant, Occupant.SpawnSettings<YellowPipeBugSpawnSettings>(), Place, Place.RequireSpeciesState<YellowPipeBugState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceYellowPipeBugTypedAnimation(Place, Place.RequireSpeciesState<YellowPipeBugState>());
}
