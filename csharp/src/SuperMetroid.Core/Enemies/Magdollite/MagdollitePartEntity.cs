namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One of a Magdollite's three placed parts (#2342): its head, rising body or tracking overlay, each its own entity with
/// its own state, sharing the reactions a hit on any part passes to the parts after it.
/// </summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="followUpRecipients">What the enemy's setup built (#2554).</param>
internal abstract partial class MagdollitePartEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, IReadOnlyList<RoomEnemyPlace> followUpRecipients)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Magdollite species owner.</summary>
    private protected MagdolliteBehavior Species => species;

    /// <summary>The enemies, up to two, the part's follow-up to a hit reaches, found as it is built (#2558).</summary>
    private IReadOnlyList<RoomEnemyPlace> FollowUpRecipients { get; } = followUpRecipients;

    /// <summary>The part's own shot reaction, which passes its fate to the enemies its follow-up reaches.</summary>
    private EnemyShotReaction OwnShot => MagdolliteBehavior.MagdolliteShotReaction(this);

    /// <summary>The part's own bomb stages, which pass its fate to the enemies its follow-up reaches.</summary>
    private EnemyBombReaction OwnBomb => MagdolliteBehavior.MagdolliteBombReaction(this);

    /// <summary>The part's own Power Bomb reaction, which passes its fate to the enemies its follow-up reaches.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => MagdolliteBehavior.MagdollitePowerBombReaction(this);

    /// <summary>Passes the part's fate on to the enemies its follow-up reaches, after a shot's common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void PassFateOn(EnemyShotHitContext context) =>
        MagdolliteBehavior.ResolveMagdolliteCombatAfterCommon(Ensure.NotNull(context).Enemy, FollowUpRecipients);

    /// <summary>Passes the part's fate on to the enemies its follow-up reaches, after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void PassFateOn(EnemyBombHitContext context) =>
        MagdolliteBehavior.ResolveMagdolliteCombatAfterCommon(Ensure.NotNull(context).Enemy, FollowUpRecipients);

    /// <summary>Passes the part's fate on to the enemies its follow-up reaches, after a Power Bomb's common damage.</summary>
    /// <param name="context">The blast meeting the part.</param>
    internal void PassFateOn(EnemyPowerBombContext context) =>
        MagdolliteBehavior.ResolveMagdolliteCombatAfterCommon(context.Enemy, FollowUpRecipients);

    /// <summary>The Magdollite the part belongs to, which it holds from its setup.</summary>
    internal abstract MagdollitePartners Partners { get; }

    /// <summary>The part's state, which it holds from its setup (#2554).</summary>
    internal abstract MagdollitePartState State { get; }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>Whether the Magdollite's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == MagdolliteReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Magdollite's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == MagdolliteReactions.Touch)
            Species.ReactToMagdolliteTouch(context, FollowUpRecipients);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Magdollite's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MagdolliteReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Magdollite's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == MagdolliteReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Magdollite's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The Magdollite's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MagdolliteReactions.Shot;
}
