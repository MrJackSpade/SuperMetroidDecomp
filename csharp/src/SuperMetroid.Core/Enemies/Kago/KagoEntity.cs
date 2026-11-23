namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kago species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells it: the hits it takes.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    KagoSpawnSettings settings, KagoEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Kago species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells it: the hits it takes.</param>
    internal KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        KagoSpawnSettings settings)
        : this(species, occupant, place, common, settings, KagoBehavior.InitializeKago(place))
    {
    }

    /// <summary>The Kago's own shot reaction, built once for this Kago.</summary>
    private EnemyShotReaction OwnShot => field ??= KagoBehavior.KagoShotReaction(ResolveShellAfterCommon);

    /// <summary>The Kago's own stages around common bomb damage, built once for this Kago.</summary>
    private EnemyBombReaction OwnBomb => field ??= KagoBehavior.KagoBombReaction(ResolveShellAfterCommon);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly KagoEnemyState state = state;

    /// <summary>Applies Kago's shell response after a bomb's common damage on this Kago.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void ResolveShellAfterCommon(EnemyBombHitContext context) =>
        species.ResolveKagoShotAfterCommon(Occupant, settings, Ensure.NotNull(context).Enemy, state);

    /// <summary>Applies Kago's shell response after a shot's common damage on this Kago.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void ResolveShellAfterCommon(EnemyShotHitContext context) =>
        species.ResolveKagoShotAfterCommon(Occupant, settings, Ensure.NotNull(context).Enemy, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Place, state);

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
