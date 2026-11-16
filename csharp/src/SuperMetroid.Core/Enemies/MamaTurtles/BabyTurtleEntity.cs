namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One of the mama turtle's babies (#2146): its crawl, its animation's cues and its own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="place">The place the baby occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TurtleEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => MamaTurtleBehavior.InitializeBabyTurtle(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBabyTurtleMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceBabyTurtleTypedAnimation(Place, context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToBabyTurtleTouch(context);
}
