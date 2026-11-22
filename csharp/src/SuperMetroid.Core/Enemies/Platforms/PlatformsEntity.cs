namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Platforms occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Platforms species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class PlatformsEntity(PlatformsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PlatformSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The species' TripperShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnTripperShotReaction => field ??= PlatformsBehavior.TripperShotReaction(ShowFrozenFrame);

    /// <summary>The species' TripperBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnTripperBombReaction => field ??= PlatformsBehavior.TripperBombReaction(ShowFrozenFrame);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal PlatformSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly PlatformEnemyState state = PlatformsBehavior.InitializePlatform(settings, place, occupant.DefinitionId);

    /// <summary>Shows a frozen Tripper's frame for its movement after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void ShowFrozenFrame(EnemyBombHitContext context)
    {
        if (Ensure.NotNull(context).Enemy.Freeze.IsRunning)
            context.Enemy.Frame = PlatformsBehavior.TripperFrozenFrame(state.XMovement);
    }

    /// <summary>Shows a Tripper's frozen frame for its movement after a shot froze it at once.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void ShowFrozenFrame(EnemyShotHitContext context)
    {
        if (Ensure.NotNull(context).Froze)
            context.Enemy.Frame = PlatformsBehavior.TripperFrozenFrame(state.XMovement);
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPlatformsMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PlatformsBehavior.AdvancePlatformsTypedAnimation(Place, state);

    /// <summary>The Tripper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PlatformsReactions.TripperShot ? OwnTripperShotReaction : base.ShotReaction;

    /// <summary>The Tripper's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PlatformsReactions.TripperShot ? OwnTripperBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Tripper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == PlatformsReactions.TripperShot && Occupant.Definition.Shot == PlatformsReactions.TripperShot;
}
