namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One of the mama turtle's babies (#2146): its crawl, its animation's cues and its own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the baby occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TurtleEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.InitializeBabyTurtle(Occupant, Occupant.SpawnSettings<BabyTurtleSpawnSettings>(), Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBabyTurtleMainAi(Place, Place.RequireSpeciesState<BabyTurtleEnemyState>(), frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceBabyTurtleTypedAnimation(Place, Place.RequireSpeciesState<BabyTurtleEnemyState>(), context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToBabyTurtleTouch(context, context.Place.RequireSpeciesState<BabyTurtleEnemyState>());
}
