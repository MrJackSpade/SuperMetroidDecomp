namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// A corpse occupant's reactions (#2557), which the dead Sidehopper, the dead Torizo and the Tourian corpses share: where
/// its definition leaves Samus's touch, shots or the Power Bomb to the species, the corpse's own reaction runs, and
/// otherwise the shared one; a bomb whose hit selects the corpse's own shot reaction runs the corpse's own bomb reaction.
/// </summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CorpseEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyPrivateShotReaction
{
    /// <summary>Runs the corpse's own touch.</summary>
    /// <param name="context">The touch.</param>
    private protected abstract void ReactToOwnTouch(EnemyTouchContext context);

    /// <summary>The corpse's own shot reaction.</summary>
    private protected abstract EnemyShotReaction OwnShotReaction { get; }

    /// <summary>The corpse's own Power Bomb reaction.</summary>
    private protected abstract EnemyPowerBombReaction OwnPowerBombReaction { get; }

    /// <summary>Runs the corpse's own bomb reaction in place of common bomb damage.</summary>
    /// <param name="context">The bomb contact.</param>
    private protected abstract void ReplaceBombDamage(EnemyBombContext context);

    /// <summary>Whether the corpse's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the corpse's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            ReactToOwnTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The corpse's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>Runs the corpse's own bomb reaction in place of common bomb damage, for a bomb whose hit selects the corpse's own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (!(context.SelectedShot is null))
            return base.TryReplaceCommonBombDamage(context);
        ReplaceBombDamage(context);
        return true;
    }

    /// <summary>The corpse's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBombReaction : base.PowerBombReaction;

    /// <summary>The corpse's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
