namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One of the mama turtle's babies (#2146): its crawl, its animation's cues and its own touch.</summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the baby occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BabyTurtleEnemyState state)
    : TurtleEntity(species, occupant, place, common), IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The mama turtle species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the baby occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    /// <param name="family">The family of the Mama the room lists before the baby, which she offered it as she was placed (#2554).</param>
    internal BabyTurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BabyTurtleSpawnSettings settings, MamaTurtleFamily family)
        : this(species, occupant, place, common, MamaTurtleBehavior.InitializeBabyTurtle(settings, place, family))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BabyTurtleEnemyState state = state;

    /// <summary>The baby's own shot reaction, which wakes its Mama.</summary>
    private EnemyShotReaction OwnShot => field ??= MamaTurtleBehavior.BabyTurtleShotReaction(this);

    /// <summary>The baby's own bomb stages, which wake its Mama.</summary>
    private EnemyBombReaction OwnBomb => field ??= MamaTurtleBehavior.BabyTurtleBombReaction(this);

    /// <summary>Wakes the baby's Mama after a shot's common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void WakeMama(EnemyShotHitContext context) =>
        state.Family.State.Wake();

    /// <summary>Wakes the baby's Mama after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void WakeMama(EnemyBombHitContext context) =>
        state.Family.State.Wake();

    /// <summary>The baby turtle's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == TurtleReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The baby turtle's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == TurtleReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The baby turtle's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == TurtleReactions.Shot && Occupant.Definition.Shot == TurtleReactions.Shot;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBabyTurtleMainAi(Place, state, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceBabyTurtleTypedAnimation(Place, state, context);

    /// <inheritdoc/>
    protected override void ReactToOwnTouch(EnemyTouchContext context) => Species.ReactToBabyTurtleTouch(context, state);
}
