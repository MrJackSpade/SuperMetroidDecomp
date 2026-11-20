namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YappingMawEntity(YappingMawBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal YappingMawEnemyState State { get; } = species.InitializeYappingMaw(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYappingMawMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYappingMawTypedAnimation(Place, context);

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
    public override void RunFrozen() => YappingMawBehavior.RunYappingMawFrozen(Place, State);

    /// <summary>The Yapping Maw's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
