namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kago species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal KagoEnemyState State { get; } = KagoBehavior.InitializeKago(place);

    /// <inheritdoc/>
    /// <remarks>The Kago has no main behaviour (#1814): its animation plays the shell and its shot reaction does the rest.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Place, State, context);

    /// <summary>The Kago's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.KagoShotReaction : base.ShotReaction;

    /// <summary>The Kago's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.KagoBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Kago's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
