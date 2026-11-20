namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kago species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells it: the hits it takes.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KagoSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Kago's own shot reaction, built for this occupant.</summary>
    private EnemyShotReaction OwnShot => field ??= species.KagoShotReaction(Occupant, settings, State);

    /// <summary>The Kago's own stages around common bomb damage, built for this occupant.</summary>
    private EnemyBombReaction OwnBomb => field ??= species.KagoBombReaction(Occupant, settings, State);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal KagoEnemyState State { get; } = KagoBehavior.InitializeKago(place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Place, State);

    /// <summary>The Kago's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == KagoReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Kago's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == KagoReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Kago's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == KagoReactions.Shot;
}
