namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, DeadSidehopperSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <summary>The species' DeadSidehopperShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnDeadSidehopperShotReaction => field ??= DeadSidehopperBehavior.DeadSidehopperShotReaction(State);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal DeadSidehopperSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(Place, State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DeadSidehopperEnemyState State { get; } = species.InitializeDeadSidehopper(occupant, settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadSidehopperMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadSidehopperBehavior.AdvanceDeadSidehopperTypedAnimation(Place, State);

    /// <summary>Whether the dead Sidehopper's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == CorpseReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the dead Sidehopper's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == CorpseReactions.Touch)
            species.ReactToDeadSidehopperTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Sidehopper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == CorpseReactions.Shot ? OwnDeadSidehopperShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Sidehopper's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != CorpseReactions.Shot)
            return base.TryReplaceCommonBombDamage(context);
        DeadSidehopperBehavior.ReplaceDeadSidehopperBombDamage(context, State);
        return true;
    }

    /// <summary>The dead Sidehopper's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The corpse's own Power Bomb reaction, built once for this corpse with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= species.DeadSidehopperPowerBombReaction(State);

    /// <summary>The dead Sidehopper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == CorpseReactions.Shot;
}
