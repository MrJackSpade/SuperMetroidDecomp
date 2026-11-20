namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One of the mama turtle's babies (#2146): its crawl, its animation's cues and its own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the baby occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BabyTurtleSpawnSettings settings)
    : TurtleEntity(species, occupant, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal BabyTurtleSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal BabyTurtleEnemyState State { get; } = species.InitializeBabyTurtle(occupant, settings, place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBabyTurtleMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceBabyTurtleTypedAnimation(Place, State, context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToBabyTurtleTouch(context, State);
}
