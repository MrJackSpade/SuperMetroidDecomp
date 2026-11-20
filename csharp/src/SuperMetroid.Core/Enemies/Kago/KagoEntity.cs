namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kago species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Kago's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.KagoShotReaction(occupant);

    /// <summary>The Kago's own stages around common bomb damage, built for this occupant.</summary>
    private readonly EnemyBombReaction ownBomb = species.KagoBombReaction(occupant);

    /// <inheritdoc/>
    public void Initialize() => KagoBehavior.InitializeKago(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Place);

    /// <summary>The Kago's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? ownShot : base.ShotReaction;

    /// <summary>The Kago's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? ownBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Kago's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
