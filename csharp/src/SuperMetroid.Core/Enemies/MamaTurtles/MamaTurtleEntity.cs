namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle (#2146): her hover, fall and crawl, her animation's cues and her own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place she occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, MamaTurtleEnemyState state)
    : TurtleEntity(species, occupant, place, common)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The mama turtle species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place she occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, MamaTurtleBehavior.InitializeMamaTurtle(place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal MamaTurtleEnemyState State { get; } = state;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunMamaTurtleMainAi(Occupant, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceMamaTurtleTypedAnimation(Place, State);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToMamaTurtleTouch(context, State);
}
