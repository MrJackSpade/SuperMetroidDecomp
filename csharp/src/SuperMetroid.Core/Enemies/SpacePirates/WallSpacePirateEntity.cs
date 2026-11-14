namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One wall Space Pirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The wall Space Pirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WallSpacePirateEntity(WallSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => WallSpacePirateBehavior.InitializeWallSpacePirate(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => WallSpacePirateBehavior.RunWallSpacePirateMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceWallSpacePirateTypedAnimation(Slot, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => SpacePirateHitboxes.Find(Slot, probe, out reaction);

    /// <summary>Pirate contact and shots both meet the Pirate's engine-owned frame hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Every ordinary Pirate's touch reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => SpacePirateReactions.Touch;

    /// <summary>Every ordinary Pirate's shot reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesShot => SpacePirateReactions.Shot;

    /// <summary>Whether the Pirates' own touch, where the definition leaves Samus's touch to them, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Pirates' own touch where the definition leaves Samus's touch to them, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.Walking.ReactToSpacePirateTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Pirates' own shot reaction where the definition leaves shots to them, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.Walking.SpacePirateShotReaction : base.ShotReaction;

    /// <summary>The Pirates' own stages around the common bomb damage every bomb that reaches an ordinary Pirate runs.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        WalkingSpacePirateBehavior.SpacePirateBombReaction;

    /// <summary>The Pirate shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        SpacePirateHitboxes.OwnsShotReaction(reaction);
}
