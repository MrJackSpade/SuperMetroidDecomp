namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle (#2146): her hover, fall and crawl, her animation's cues and her own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="place">The place she occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TurtleEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MamaTurtleEnemyState State { get; } = MamaTurtleBehavior.InitializeMamaTurtle(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunMamaTurtleMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceMamaTurtleTypedAnimation(Place, State, context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToMamaTurtleTouch(context, State);
}
