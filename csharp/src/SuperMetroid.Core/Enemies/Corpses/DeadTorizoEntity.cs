namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Torizo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The dead Torizo species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTorizoBehavior.ShownDeadTorizoRows(Place);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal DeadTorizoEnemyState State { get; } = species.InitializeDeadTorizo(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTorizoMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DeadTorizoBehavior.AdvanceDeadTorizoTypedAnimation(Place, context);

    /// <summary>Whether the dead Torizo's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the dead Torizo's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            DeadTorizoBehavior.ReactToDeadTorizoTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Torizo's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.DeadTorizoShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Torizo's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (!(context.SelectedShot is null))
            return base.TryReplaceCommonBombDamage(context);
        DeadTorizoBehavior.ReplaceDeadTorizoBombDamage(context);
        return true;
    }

    /// <summary>The dead Torizo's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.DeadTorizoPowerBombReaction : base.PowerBombReaction;

    /// <summary>The dead Torizo's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
