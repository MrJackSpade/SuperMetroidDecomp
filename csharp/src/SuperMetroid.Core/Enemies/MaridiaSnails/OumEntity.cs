namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One Oum occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Oum species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class OumEntity(OumBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal OumEnemyState State { get; } = OumBehavior.InitializeOum();

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunOumMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceOumTypedAnimation(Place, State, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => OumBehavior.FindHitboxReaction(Place, probe, out reaction);

    /// <summary>The large snail's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The large snail's harmless touch, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => OumReactions.NonDamagingTouch;

    /// <summary>The large snail's shot reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesShot => OumReactions.Shot;

    /// <summary>Whether the snail's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the snail's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToOumTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The snail's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.OumShotReaction : base.ShotReaction;

    /// <summary>The snail's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == OumReactions.Shot
            ? species.OumBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The large snail's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == OumReactions.Shot;
}
