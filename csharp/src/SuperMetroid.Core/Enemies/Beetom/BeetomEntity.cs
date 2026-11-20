namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One Beetom occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Beetom species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
internal sealed class BeetomEntity(BeetomBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal BeetomEnemyState State { get; } = species.InitializeBeetom(place, Ensure.NotNull(surroundings).Samus, surroundings.ControllerInput);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBeetomMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BeetomBehavior.AdvanceBeetomTypedAnimation(Place, State);

    /// <summary>Whether the Beetom's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == BeetomReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Beetom's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == BeetomReactions.Touch)
            species.ReactToBeetomTouch(context, context.State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Beetom's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BeetomReactions.Shot ? BeetomBehavior.BeetomShotReaction : base.ShotReaction;

    /// <summary>The Beetom's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == BeetomReactions.Shot ? BeetomBehavior.BeetomBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Beetom's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BeetomReactions.Shot;
}
