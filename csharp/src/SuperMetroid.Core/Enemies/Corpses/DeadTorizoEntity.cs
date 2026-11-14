namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => species.ShownDeadTorizoRows(Slot);

    /// <inheritdoc/>
    public void Initialize() => species.InitializeDeadTorizo(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunDeadTorizoMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceDeadTorizoTypedAnimation(Slot, context);

    /// <summary>Whether the dead Torizo's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the dead Torizo's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToDeadTorizoTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The dead Torizo's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.DeadTorizoShotReaction : base.ShotReaction;

    /// <summary>Runs the dead Torizo's its own bomb reaction in place of common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (!(context.SelectedShot is null))
            return base.TryReplaceCommonBombDamage(context);
        species.ReplaceDeadTorizoBombDamage(context);
        return true;
    }

    /// <summary>The dead Torizo's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.DeadTorizoPowerBombReaction : base.PowerBombReaction;

    /// <summary>The dead Torizo's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
