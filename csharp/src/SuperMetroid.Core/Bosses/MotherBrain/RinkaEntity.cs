using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Rinka occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Rinka species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RinkaEntity(RinkaBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeRinka(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunRinkaMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RinkaBehavior.AdvanceRinkaTypedAnimation(Slot, context);

    /// <summary>Whether Rinka's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs Rinka's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToRinkaTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Rinka's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.RinkaShotReaction : base.ShotReaction;

    /// <summary>Rinka's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.RinkaBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Rinka's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.RinkaPowerBombReaction : base.PowerBombReaction;

    /// <summary>Runs Rinka's frozen tail before the common thaw; true when it ended the turn.</summary>
    public override bool PreemptFrozenTurn() => species.RunRinkaFrozenTail(Slot);

    /// <summary>Rinka's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
