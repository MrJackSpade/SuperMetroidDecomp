namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Metroid occupant: its own AI and reactions, with the shared ones its definition names.</summary>
/// <param name="species">The Metroid species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MetroidEntity(MetroidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeMetroid(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMetroidMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceMetroidTypedAnimation(Place, context);

    /// <summary>The Metroid's own hurt behaviour, ahead of frozen AI when both states are active; the animation still advances.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        MetroidBehavior.ApplyMetroidHurt(Place);
        return true;
    }

    /// <summary>The frozen Metroid's behaviour, which retains its outer-body state.</summary>
    public override void RunFrozen() => MetroidBehavior.RunMetroidFrozen(Place);

    /// <summary>Whether the Metroid's own touch, its attachment and suit-dependent draining, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Metroid's own touch where its definition leaves the touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ResolveMetroidTouch(context.Place, context.Samus);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Metroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.ShotReaction : base.ShotReaction;

    /// <summary>The Metroid's own bomb reaction, which comes before the ignoring check, where its definition leaves shots to it.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!LeavesShotToSpecies)
            return base.TryReactToBomb(context);
        species.ReactToMetroidBomb(context);
        return true;
    }

    /// <summary>The Metroid's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? MetroidBehavior.PowerBombReaction : base.PowerBombReaction;

    /// <summary>The Metroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path selects (none: the definition leaves shots to the Metroid), or the empty frame's.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
