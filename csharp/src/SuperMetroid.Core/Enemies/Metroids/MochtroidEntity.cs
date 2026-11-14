namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Mochtroid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Mochtroid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MochtroidEntity(MochtroidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => MochtroidBehavior.InitializeMochtroid(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMochtroidMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MochtroidBehavior.AdvanceMochtroidTypedAnimation(Slot, context);

    /// <summary>Whether the Mochtroid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Mochtroid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToMochtroidTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Mochtroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? MochtroidBehavior.MochtroidShotReaction : base.ShotReaction;

    /// <summary>The Mochtroid's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
