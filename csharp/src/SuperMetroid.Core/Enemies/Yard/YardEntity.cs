namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One Yard occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Yard species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YardEntity(YardEnemyBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal YardEnemyState State { get; } = YardEnemyBehavior.InitializeYard(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYardMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYardTypedAnimation(Place, State, context);

    /// <summary>Whether the Yard's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Yard's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToYardTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Yard's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.YardShotReaction : base.ShotReaction;

    /// <summary>The Yard's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
