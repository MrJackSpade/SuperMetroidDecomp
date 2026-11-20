namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Tourian corpse occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Tourian corpses' species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTourianBehavior.ShownDeadTourianRows(Place);

    /// <inheritdoc/>
    public void Initialize() => DeadTourianBehavior.InitializeDeadTourianCorpse(Place, Occupant);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTourianMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation(Place);

    /// <summary>Whether the dead Tourian corpse's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Place.Definition.Touch == CorpseReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the dead Tourian corpse's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Place.Definition.Touch == CorpseReactions.Touch)
            DeadTourianBehavior.ReactToDeadTourianTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Tourian corpse's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Place.Definition.Shot == CorpseReactions.Shot ? DeadTourianBehavior.DeadTourianShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Tourian corpse's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != CorpseReactions.Shot)
            return base.TryReplaceCommonBombDamage(context);
        DeadTourianBehavior.ReplaceDeadTourianBombDamage(context);
        return true;
    }

    /// <summary>The dead Tourian corpse's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? DeadTourianBehavior.DeadTourianPowerBombReaction : base.PowerBombReaction;

    /// <summary>The dead Tourian corpse's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == CorpseReactions.Shot;
}
