namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon's cosmetic wing (#2175), which shares the Dragon's definition: its placement names it the wing of the body its
/// room lists right before it, which moves it and starts its flapping. It has no AI of its own, and keeps the Dragon's
/// reactions where its definition leaves them to it; its intangibility normally keeps Samus and her shots from reaching it.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wing occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">The wing's state: the one its body began as it was placed, or, for a wing no body was listed before, its own.</param>
internal sealed class DragonWingEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    DragonWingState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DragonWingState State { get; } = DragonBehavior.InitializeDragonWing(place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonWingTypedAnimation(Place, State);

    /// <summary>The enemy the part's follow-up to a hit reaches, the one its room lists right after it, found as it is built.</summary>
    private RoomEnemyPlace FollowUpRecipient { get; } = species.FollowUpRecipientOf(place);

    /// <summary>Whether the Dragon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == DragonReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Dragon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == DragonReactions.Touch)
            species.ReactToDragonTouch(context, FollowUpRecipient);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The part's own shot reaction, which carries its hit to the enemy its follow-up reaches.</summary>
    private EnemyShotReaction OwnShot => field ??= DragonBehavior.DragonShotReaction(FollowUpRecipient);

    /// <summary>The part's own bomb stages, which carry its hit to the enemy its follow-up reaches.</summary>
    private EnemyBombReaction OwnBomb => field ??= DragonBehavior.DragonBombReaction(FollowUpRecipient);

    /// <summary>The part's own Power Bomb reaction, which carries its hit to the enemy its follow-up reaches.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= DragonBehavior.DragonPowerBombReaction(FollowUpRecipient);

    /// <summary>The Dragon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == DragonReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Dragon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == DragonReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Dragon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The Dragon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == DragonReactions.Shot;
}
