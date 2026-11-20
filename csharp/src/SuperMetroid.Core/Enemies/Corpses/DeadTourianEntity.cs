namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Tourian corpse occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Tourian corpses' species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CorpseSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <summary>The enemy's own Power Bomb reaction, built over its state (#2554).</summary>
    private EnemyPowerBombReaction OwnDeadTourianPowerBombReaction => field ??= DeadTourianBehavior.DeadTourianPowerBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnDeadTourianShotReaction => field ??= DeadTourianBehavior.DeadTourianShotReactionOf(State);

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTourianBehavior.ShownDeadTourianRows(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal DeadTourianCorpseEnemyState State { get; } = DeadTourianBehavior.InitializeDeadTourianCorpse(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTourianMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation(Place, State, context);

    /// <summary>Whether the dead Tourian corpse's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == CorpseReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the dead Tourian corpse's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == CorpseReactions.Touch)
            DeadTourianBehavior.ReactToDeadTourianTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Tourian corpse's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == CorpseReactions.Shot ? OwnDeadTourianShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Tourian corpse's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (!(context.SelectedShot is null))
            return base.TryReplaceCommonBombDamage(context);
        DeadTourianBehavior.ReplaceDeadTourianBombDamage(context, State);
        return true;
    }

    /// <summary>The dead Tourian corpse's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnDeadTourianPowerBombReaction : base.PowerBombReaction;

    /// <summary>The dead Tourian corpse's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == CorpseReactions.Shot;
}
