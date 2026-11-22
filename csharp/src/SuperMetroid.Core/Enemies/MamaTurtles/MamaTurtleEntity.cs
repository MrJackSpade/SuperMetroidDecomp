namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle (#2146): her hover, fall and crawl, her animation's cues and her own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place she occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TurtleEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.InitializeMamaTurtle(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunMamaTurtleMainAi(Occupant, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceMamaTurtleTypedAnimation(Place);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToMamaTurtleTouch(context);
}
