namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One Maridia large snail occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Maridia large snail species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MaridiaLargeSnailEntity(MaridiaLargeSnailBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMaridiaLargeSnailMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceMaridiaLargeSnailTypedAnimation(Slot, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => MaridiaLargeSnailBehavior.FindHitboxReaction(Slot, probe, out reaction);

    /// <summary>The large snail's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The large snail's harmless touch, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => MaridiaLargeSnailReactions.NonDamagingTouch;

    /// <summary>The large snail's shot reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesShot => MaridiaLargeSnailReactions.Shot;

    /// <summary>Whether the snail's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the snail's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToMaridiaLargeSnailTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The snail's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.MaridiaLargeSnailShotReaction : base.ShotReaction;

    /// <summary>The snail's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == MaridiaLargeSnailReactions.Shot
            ? species.MaridiaLargeSnailBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The large snail's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == MaridiaLargeSnailReactions.Shot;
}
