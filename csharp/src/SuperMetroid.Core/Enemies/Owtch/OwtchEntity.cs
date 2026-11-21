namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One Owtch occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Owtch species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class OwtchEntity(OwtchBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, OwtchSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The species' OwtchShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnOwtchShotReaction => field ??= OwtchBehavior.OwtchShotReaction(State);

    /// <summary>The species' OwtchBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnOwtchBombReaction => field ??= OwtchBehavior.OwtchBombReaction(State);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal OwtchSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal OwtchEnemyState State { get; } = OwtchBehavior.InitializeOwtch(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunOwtchMainAi(Settings, Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => OwtchBehavior.AdvanceOwtchTypedAnimation(Place, State);

    /// <summary>The Owtch's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == OwtchReactions.Shot ? OwnOwtchShotReaction : base.ShotReaction;

    /// <summary>The Owtch's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == OwtchReactions.Shot ? OwnOwtchBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Owtch's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == OwtchReactions.Shot;
}
