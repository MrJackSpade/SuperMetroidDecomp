namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One of the mama turtle's babies (#2146): its crawl, its animation's cues, its own touch and its own shot and bomb stages.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="place">The place the baby occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="start">The Mama that heads the baby and the way it first crawls.</param>
internal sealed class BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BabyTurtleStart start)
    : TurtleEntity(species, place, common), IEnemyPrivateShotReaction
{
    /// <summary>The baby's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnShotReaction => field ??= MamaTurtleBehavior.BabyTurtleShotReactionOf(State);

    /// <summary>The baby's own bomb stages, built over its state (#2554).</summary>
    private EnemyBombReaction OwnBombReaction => field ??= MamaTurtleBehavior.BabyTurtleBombReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BabyTurtleEnemyState State { get; } = MamaTurtleBehavior.InitializeBabyTurtle(place, start);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBabyTurtleMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceBabyTurtleTypedAnimation(Place, State, context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToBabyTurtleTouch(context, State);

    /// <summary>The baby turtle's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>The baby turtle's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The baby turtle's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
