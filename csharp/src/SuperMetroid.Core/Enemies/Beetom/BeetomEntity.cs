namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One Beetom occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Beetom species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class BeetomEntity(BeetomBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    BeetomEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Beetom species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    internal BeetomEntity(BeetomBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
        : this(species, occupant, place, common, species.InitializeBeetom(place, Ensure.NotNull(surroundings).Samus, surroundings.ControllerInput))
    {
    }

    /// <summary>The species' BeetomShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnBeetomShotReaction => field ??= BeetomBehavior.BeetomShotReaction(LetGo);

    /// <summary>The species' BeetomBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnBeetomBombReaction => field ??= BeetomBehavior.BeetomBombReaction(LetGo);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BeetomEnemyState state = state;

    /// <summary>Lets the Beetom go of Samus after a shot's common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void LetGo(EnemyShotHitContext context) =>
        BeetomBehavior.ResolveBeetomShotAfterCommon(Ensure.NotNull(context).Enemy, state);

    /// <summary>Lets the Beetom go of Samus after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void LetGo(EnemyBombHitContext context) =>
        BeetomBehavior.ResolveBeetomShotAfterCommon(Ensure.NotNull(context).Enemy, state);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBeetomMainAi(Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BeetomBehavior.AdvanceBeetomTypedAnimation(Place, state);

    /// <summary>Whether the Beetom's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == BeetomReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Beetom's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == BeetomReactions.Touch)
            species.ReactToBeetomTouch(context, state);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Beetom's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BeetomReactions.Shot ? OwnBeetomShotReaction : base.ShotReaction;

    /// <summary>The Beetom's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == BeetomReactions.Shot ? OwnBeetomBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Beetom's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BeetomReactions.Shot;
}
