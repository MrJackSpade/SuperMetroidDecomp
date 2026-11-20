namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Platforms occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Platforms species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class PlatformsEntity(PlatformsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PlatformSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnTripperBombReaction => field ??= PlatformsBehavior.TripperBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnTripperShotReaction => field ??= PlatformsBehavior.TripperShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal PlatformEnemyState State { get; } = PlatformsBehavior.InitializePlatform(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPlatformsMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PlatformsBehavior.AdvancePlatformsTypedAnimation(Place, State, context);

    /// <summary>The Tripper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PlatformsReactions.TripperShot ? OwnTripperShotReaction : base.ShotReaction;

    /// <summary>The Tripper's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PlatformsReactions.TripperShot ? OwnTripperBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Tripper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PlatformsReactions.TripperShot;
}
