namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>One Fireflea occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fireflea species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FirefleaEntity(FirefleaBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => FirefleaBehavior.InitializeFireflea(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FirefleaBehavior.RunFirefleaMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => FirefleaBehavior.AdvanceFirefleaTypedAnimation(Slot, context);

    /// <summary>Whether the Fireflea's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Fireflea's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToFirefleaTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Fireflea's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.FirefleaShotReaction : base.ShotReaction;

    /// <summary>The Fireflea's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.FirefleaBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Fireflea's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.FirefleaPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Fireflea's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
