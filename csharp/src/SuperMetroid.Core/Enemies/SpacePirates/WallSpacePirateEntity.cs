namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One wall Space Pirate occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The wall Space Pirate species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class WallSpacePirateEntity(WallSpacePirateBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, WallSpacePirateSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal WallSpacePirateSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal WallSpacePirateEnemyState State { get; } = WallSpacePirateBehavior.InitializeWallSpacePirate(occupant, settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => WallSpacePirateBehavior.RunWallSpacePirateMainAi(Occupant, Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceWallSpacePirateTypedAnimation(Occupant, Settings, Place, State, context);

    /// <inheritdoc/>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) => SpacePirateHitboxes.Find(Place, probe);

    /// <summary>Pirate contact and shots both meet the frame hitboxes the game defines for the Pirate.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Whether the Pirates' own touch, where the definition leaves Samus's touch to them, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == SpacePirateReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Pirates' own touch where the definition leaves Samus's touch to them, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == SpacePirateReactions.Touch)
            species.Walking.ReactToSpacePirateTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Pirates' own shot reaction where the definition leaves shots to them, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SpacePirateReactions.Shot ? species.Walking.SpacePirateShotReaction : base.ShotReaction;

    /// <summary>The Pirates' own stages around the common bomb damage every bomb that reaches an ordinary Pirate runs.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        WalkingSpacePirateBehavior.SpacePirateBombReaction;

    /// <summary>The Pirate shot reactions, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        SpacePirateHitboxes.OwnsShotReaction(reaction);
}
