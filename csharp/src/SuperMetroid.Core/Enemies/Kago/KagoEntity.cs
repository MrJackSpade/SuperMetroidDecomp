namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kago species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => KagoBehavior.InitializeKago(Slot);

    /// <inheritdoc/>
    /// <remarks>The Kago has no main behaviour (#1814): its animation plays the shell and its shot reaction does the rest.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Slot, context);

    /// <summary>The Kago's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.KagoShotReaction : base.ShotReaction;

    /// <summary>The Kago's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.KagoBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Kago's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
