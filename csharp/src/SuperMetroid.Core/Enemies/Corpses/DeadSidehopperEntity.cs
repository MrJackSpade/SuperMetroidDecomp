namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(Place, Place.RequireSpeciesState<DeadSidehopperEnemyState>());

    /// <inheritdoc/>
    public void Initialize() => species.InitializeDeadSidehopper(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadSidehopperMainAi(Place, Place.RequireSpeciesState<DeadSidehopperEnemyState>(), frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadSidehopperBehavior.AdvanceDeadSidehopperTypedAnimation(Place, Place.RequireSpeciesState<DeadSidehopperEnemyState>(), Place.RequireSpeciesState<DeadSidehopperEnemyState>());

    /// <summary>Whether the dead Sidehopper's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == CorpseReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the dead Sidehopper's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == CorpseReactions.Touch)
            species.ReactToDeadSidehopperTouch(context, context.Place.RequireSpeciesState<DeadSidehopperEnemyState>());
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Sidehopper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == CorpseReactions.Shot ? DeadSidehopperBehavior.DeadSidehopperShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Sidehopper's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != CorpseReactions.Shot)
            return base.TryReplaceCommonBombDamage(context);
        DeadSidehopperBehavior.ReplaceDeadSidehopperBombDamage(context, context.Enemy.RequireSpeciesState<DeadSidehopperEnemyState>());
        return true;
    }

    /// <summary>The dead Sidehopper's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.DeadSidehopperPowerBombReaction : base.PowerBombReaction;

    /// <summary>The dead Sidehopper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == CorpseReactions.Shot;
}
