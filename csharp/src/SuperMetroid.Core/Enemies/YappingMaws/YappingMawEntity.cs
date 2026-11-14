namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YappingMawEntity(YappingMawBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeYappingMaw(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYappingMawMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYappingMawTypedAnimation(Slot, context);

    /// <summary>Whether the Yapping Maw's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Yapping Maw's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            YappingMawBehavior.ReactToYappingMawTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Yapping Maw's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? YappingMawBehavior.YappingMawShotReaction : base.ShotReaction;

    /// <summary>The Yapping Maw's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? YappingMawBehavior.YappingMawBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Runs the Yapping Maw's frozen behaviour after the common thaw.</summary>
    public override void RunFrozen() => YappingMawBehavior.RunYappingMawFrozen(Slot, Slot.RequireSpeciesState<YappingMawEnemyState>());

    /// <summary>The Yapping Maw's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
